using Microsoft.AspNetCore.SignalR;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;
using PiFanCtrl.Model.SignalR;
using PiFanCtrl.Services;
using PiFanCtrl.Services.Stores;
using PiFanCtrl.Services.Temperature;

namespace PiFanCtrl.Hubs;

public class FanControlHub : Hub<IFanControlClient>
{
  private readonly IReadingStore _readingStore;
  private readonly ReadingStoreWrapper _delegatingStore;
  private readonly DummyTemperatureSensor _dummyTemperatureSensor;
  private readonly FanSpeedCalculator _fanSpeedCalculator;
  private readonly ILogger<FanControlHub> _logger;

  public FanControlHub(
    [FromKeyedServices("delegating")] IReadingStore readingStore,
    DummyTemperatureSensor dummyTemperatureSensor,
    FanSpeedCalculator fanSpeedCalculator,
    ILogger<FanControlHub> logger)
  {
    _readingStore = readingStore;
    _delegatingStore = (ReadingStoreWrapper)readingStore;
    _dummyTemperatureSensor = dummyTemperatureSensor;
    _fanSpeedCalculator = fanSpeedCalculator;
    _logger = logger;
  }

  public override async Task OnConnectedAsync()
  {
    _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);

    // Send current values immediately on connection
    await SendCurrentValues();

    await base.OnConnectedAsync();
  }

  public override async Task OnDisconnectedAsync(Exception? exception)
  {
    if (exception != null)
    {
      _logger.LogError(exception, "Client disconnected with error: {ConnectionId}", Context.ConnectionId);
    }
    else
    {
      _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
    }
    await base.OnDisconnectedAsync(exception);
  }

  private async Task SendCurrentValues()
  {
    // Get all known sources from the delegating store
    var sources = _delegatingStore.GetKnownSources();

    foreach (var source in sources)
    {
      var reading = _readingStore.GetLatest(source);
      if (reading != null)
      {
        // Use switch expression to determine the type and send appropriate update
        await (reading switch
        {
          TemperatureReading tempReading when tempReading.Active => 
            Clients.Caller.TemperatureUpdate(new TemperatureUpdateDto(
              tempReading.Source,
              tempReading.Value,
              tempReading.AsOf
            )),
          FanRpmReading rpmReading => 
            Clients.Caller.FanRpmUpdate(new FanRpmUpdateDto(
              rpmReading.Source,
              rpmReading.Value,
              rpmReading.AsOf
            )),
          _ => LogUnexpectedReadingType(reading)
        });
      }
    }
  }

  private Task LogUnexpectedReadingType(IReading reading)
  {
    _logger.LogError("Unexpected IReading implementation received: {Type} from source {Source}", 
      reading.GetType().Name, reading.Source);
    return Task.CompletedTask;
  }

  public async Task SimulateTemperature(decimal temperature)
  {
    // Validate temperature range (-273.15°C is absolute zero, 200°C is a reasonable upper limit)
    if (temperature < -273.15m || temperature > 200m)
    {
      _logger.LogWarning("Invalid temperature value: {Temperature}. Must be between -273.15 and 200", temperature);
      throw new ArgumentOutOfRangeException(nameof(temperature), temperature, "Temperature must be between -273.15 and 200");
    }

    _logger.LogInformation("Simulating temperature: {Temperature}", temperature);
    _dummyTemperatureSensor.Simulate(temperature);
    await Clients.All.TemperatureSimulated(new TemperatureSimulatedDto(temperature));
  }

  public async Task SetFanSpeed(decimal speedPercentage)
  {
    // Validate speed percentage range
    if (speedPercentage < 0m || speedPercentage > 100m)
    {
      _logger.LogWarning("Invalid fan speed value: {Speed}. Must be between 0 and 100", speedPercentage);
      throw new ArgumentOutOfRangeException(nameof(speedPercentage), speedPercentage, "Fan speed must be between 0 and 100");
    }

    _logger.LogInformation("Setting fan speed to {Speed}%", speedPercentage);
    
    var currentSettings = _fanSpeedCalculator.FanSettings;
    var newSettings = currentSettings with
    {
      MinimumSpeed = speedPercentage,
      PanicSpeed = speedPercentage
    };

    _fanSpeedCalculator.UpdateFanSettings(newSettings);
    await Clients.All.FanSpeedSet(new FanSpeedSetDto(speedPercentage));
  }

  public async Task ResetFanSettings()
  {
    _logger.LogInformation("Resetting fan settings to original configuration");
    _fanSpeedCalculator.ResetFanSettings();
    _dummyTemperatureSensor.Reset();
    await Clients.All.FanSettingsReset(new FanSettingsResetDto());
  }

  public async Task GetCurrentTemperature()
  {
    await SendCurrentValues();
  }
}
