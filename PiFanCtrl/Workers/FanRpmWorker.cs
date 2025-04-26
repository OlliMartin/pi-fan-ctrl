using PiFanCtrl.Interfaces;

namespace PiFanCtrl.Workers;

public class FanRpmWorker(ILogger<FanRpmWorker> logger, IFanRpmSensor rpmSensor) : IHostedService
{
  private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(5));

  private CancellationTokenSource? _cts;

  public Task StartAsync(CancellationToken cancellationToken)
  {
    logger.LogInformation("Starting fan rpm worker.");

    _cts = new CancellationTokenSource();
    _ = RunTimerAsync(_cts.Token);

    return Task.CompletedTask;
  }

  private async Task RunTimerAsync(CancellationToken cancelToken = default)
  {
    try
    {
      while (await _timer.WaitForNextTickAsync(cancelToken))
      {
        var reading = await rpmSensor.ReadNextValueAsync(cancelToken);
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
    await (_cts?.CancelAsync() ?? Task.CompletedTask);
    _cts?.Dispose();
  }
}