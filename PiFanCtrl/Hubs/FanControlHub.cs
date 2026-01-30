using Microsoft.AspNetCore.SignalR;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;
using PiFanCtrl.Services;
using PiFanCtrl.Services.Stores;
using PiFanCtrl.Services.Temperature;

namespace PiFanCtrl.Hubs;

public class FanControlHub : Hub
{
  private readonly SlidingReadingStore _readingStore;
  private readonly DummyTemperatureSensor _dummyTemperatureSensor;
  private readonly FanSpeedCalculator _fanSpeedCalculator;
  private readonly ILogger<FanControlHub> _logger;

  public FanControlHub(
    SlidingReadingStore readingStore,
    DummyTemperatureSensor dummyTemperatureSensor,
    FanSpeedCalculator fanSpeedCalculator,
    ILogger<FanControlHub> logger)
  {
    _readingStore = readingStore;
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
    _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
    await base.OnDisconnectedAsync(exception);
  }

  private async Task SendCurrentValues()
  {
    // Get latest temperature
    var temperatureReadings = _readingStore.GetAll()
      .OfType<TemperatureReading>()
      .Where(r => r.Active)
      .GroupBy(r => r.Source)
      .Select(g => g.OrderByDescending(r => r.AsOf).First())
      .ToList();

    // Get latest fan RPM
    var rpmReadings = _readingStore.GetAll()
      .OfType<FanRpmReading>()
      .GroupBy(r => r.Source)
      .Select(g => g.OrderByDescending(r => r.AsOf).First())
      .ToList();

    foreach (var reading in temperatureReadings)
    {
      await Clients.Caller.SendAsync("TemperatureUpdate", new
      {
        source = reading.Source,
        value = reading.Value,
        timestamp = reading.AsOf
      });
    }

    foreach (var reading in rpmReadings)
    {
      await Clients.Caller.SendAsync("FanRpmUpdate", new
      {
        source = reading.Source,
        value = reading.Value,
        timestamp = reading.AsOf
      });
    }
  }

  public async Task SimulateTemperature(decimal temperature)
  {
    _logger.LogInformation("Simulating temperature: {Temperature}", temperature);
    _dummyTemperatureSensor.Simulate(temperature);
    await Clients.All.SendAsync("TemperatureSimulated", temperature);
  }

  public async Task SetFanSpeed(decimal speedPercentage)
  {
    _logger.LogInformation("Setting fan speed to {Speed}%", speedPercentage);
    
    var currentSettings = _fanSpeedCalculator.FanSettings;
    var newSettings = currentSettings with
    {
      MinimumSpeed = speedPercentage,
      PanicSpeed = speedPercentage
    };

    _fanSpeedCalculator.UpdateFanSettings(newSettings);
    await Clients.All.SendAsync("FanSpeedSet", speedPercentage);
  }

  public async Task ResetFanSettings()
  {
    _logger.LogInformation("Resetting fan settings to original configuration");
    _fanSpeedCalculator.ResetFanSettings();
    _dummyTemperatureSensor.Reset();
    await Clients.All.SendAsync("FanSettingsReset");
  }

  public async Task GetCurrentTemperature()
  {
    await SendCurrentValues();
  }
}
