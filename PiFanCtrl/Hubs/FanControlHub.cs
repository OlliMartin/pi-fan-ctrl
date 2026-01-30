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
      var value = _readingStore.GetLatest(source);
      if (value.HasValue)
      {
        // Determine the type based on source naming or check actual readings
        // For now, we'll check if it's a temperature or RPM based on common patterns
        if (source.Contains("Temp", StringComparison.OrdinalIgnoreCase) ||
            source == "Simulated" ||
            source == "Aggregate" ||
            source.Contains("BMP", StringComparison.OrdinalIgnoreCase))
        {
          await Clients.Caller.TemperatureUpdate(new TemperatureUpdateDto(
            source,
            value.Value,
            DateTime.UtcNow
          ));
        }
        else if (source.Contains("Rpm", StringComparison.OrdinalIgnoreCase) ||
                 source.Contains("Fan", StringComparison.OrdinalIgnoreCase))
        {
          await Clients.Caller.FanRpmUpdate(new FanRpmUpdateDto(
            source,
            value.Value,
            DateTime.UtcNow
          ));
        }
      }
    }
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
