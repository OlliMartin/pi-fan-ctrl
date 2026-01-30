using Microsoft.AspNetCore.SignalR;
using PiFanCtrl.Hubs;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;
using PiFanCtrl.Services.Stores;

namespace PiFanCtrl.Services;

public class ReadingPushService : IHostedService
{
  private readonly SlidingReadingStore _readingStore;
  private readonly IHubContext<FanControlHub> _hubContext;
  private readonly ILogger<ReadingPushService> _logger;

  public ReadingPushService(
    SlidingReadingStore readingStore,
    IHubContext<FanControlHub> hubContext,
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
      
      if (reading is TemperatureReading tempReading && tempReading.Active)
      {
        await _hubContext.Clients.All.SendAsync("TemperatureUpdate", new
        {
          source = tempReading.Source,
          value = tempReading.Value,
          timestamp = tempReading.AsOf
        });
      }
      else if (reading is FanRpmReading rpmReading)
      {
        await _hubContext.Clients.All.SendAsync("FanRpmUpdate", new
        {
          source = rpmReading.Source,
          value = rpmReading.Value,
          timestamp = rpmReading.AsOf
        });
      }
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
