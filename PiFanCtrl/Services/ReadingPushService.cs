using Microsoft.AspNetCore.SignalR;
using PiFanCtrl.Hubs;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;
using PiFanCtrl.Model.SignalR;
using PiFanCtrl.Services.Stores;

namespace PiFanCtrl.Services;

public class ReadingPushService : IHostedService
{
  private readonly IReadingStore _readingStore;
  private readonly IHubContext<FanControlHub, IFanControlClient> _hubContext;
  private readonly ILogger<ReadingPushService> _logger;

  public ReadingPushService(
    [FromKeyedServices("delegating")] IReadingStore readingStore,
    IHubContext<FanControlHub, IFanControlClient> hubContext,
    ILogger<ReadingPushService> logger)
  {
    _readingStore = readingStore;
    _hubContext = hubContext;
    _logger = logger;
  }

  public Task StartAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Starting ReadingPushService");
    _readingStore.ReadingChanged += OnReadingChanged;
    return Task.CompletedTask;
  }

  private async void OnReadingChanged(object? sender, ReadingChangedEventArgs e)
  {
    try
    {
      var reading = e.Reading;
      
      await (reading switch
      {
        TemperatureReading tempReading when tempReading.Active =>
          _hubContext.Clients.All.TemperatureUpdate(new TemperatureUpdateDto(
            tempReading.Source,
            tempReading.Value,
            tempReading.AsOf
          )),
        FanRpmReading rpmReading =>
          _hubContext.Clients.All.FanRpmUpdate(new FanRpmUpdateDto(
            rpmReading.Source,
            rpmReading.Value,
            rpmReading.AsOf
          )),
        _ => Task.CompletedTask
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error pushing reading to SignalR clients");
    }
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Stopping ReadingPushService");
    _readingStore.ReadingChanged -= OnReadingChanged;
    return Task.CompletedTask;
  }
}
