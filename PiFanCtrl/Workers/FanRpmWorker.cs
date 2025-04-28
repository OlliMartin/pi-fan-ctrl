using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;

namespace PiFanCtrl.Workers;

public class FanRpmWorker(
  ILogger<FanRpmWorker> logger,
  IFanRpmSensor rpmSensor,
  [FromKeyedServices("delegating")] IReadingStore readingStore
) : IHostedService
{
  private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(seconds: 5));

  private CancellationTokenSource? _cts;

  public Task StartAsync(CancellationToken cancellationToken)
  {
    logger.LogInformation("Starting fan rpm worker.");

    _cts = new();
    _ = RunTimerAsync(_cts.Token);

    return Task.CompletedTask;
  }

  private async Task RunTimerAsync(CancellationToken cancelToken = default)
  {
    try
    {
      while (await _timer.WaitForNextTickAsync(cancelToken))
      {
        FanRpmReading? reading = await rpmSensor.ReadNextValueAsync(cancelToken);

        if (reading is not null)
        {
          await readingStore.AddAsync(reading, cancelToken);
        }
      }
    }
    catch (OperationCanceledException)
    {
      logger.LogInformation("Stopped.");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An error occurred during pwm processing.");

      throw;
    }
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    logger.LogInformation("{wName} is stopping.", nameof(FanRpmWorker));

    try
    {
      await (_cts?.CancelAsync() ?? Task.CompletedTask);
      _cts?.Dispose();
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An error occurred stopping worker {wName}.", GetType().Name);
    }
  }
}