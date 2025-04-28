using System.Diagnostics;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;
using PiFanCtrl.Services.Pwm;

namespace PiFanCtrl.Workers;

public class PwmControlWorker(
  [FromKeyedServices("delegating")] ITemperatureSensor temperatureSensor,
  PwmControllerWrapper pwmController,
  IReadingStore readingStore,
  ILogger<PwmControlWorker> logger
) : IHostedService
{
  private const decimal DEFAULT_DUTY_CYCLE = 100;
  private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(seconds: 5));

  private CancellationTokenSource? _cts;

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    Stopwatch swStart = Stopwatch.StartNew();
    logger.LogInformation("Starting pwm control worker.");

    _cts = new();
    _ = Task.Run(() => RunTimerAsync(_cts.Token), cancellationToken);

    logger.LogInformation("Initializing duty cycle to {val}.", DEFAULT_DUTY_CYCLE);
    await pwmController.SetDutyCycleAsync(DEFAULT_DUTY_CYCLE, cancellationToken);

    swStart.Stop();
    logger.LogInformation("{wName} started in {elapsed}.", nameof(PwmControlWorker), swStart.Elapsed);
  }

  private async Task RunTimerAsync(CancellationToken cancelToken = default)
  {
    try
    {
      while (await _timer.WaitForNextTickAsync(cancelToken))
      {
        IEnumerable<TemperatureReading>? reading = await temperatureSensor.ReadNextValuesAsync(cancelToken);
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
    logger.LogInformation("{wName} is stopping.", nameof(PwmControlWorker));

    try
    {
      await pwmController.SetDutyCycleAsync(percentage: 100, CancellationToken.None);

      await (_cts?.CancelAsync() ?? Task.CompletedTask);
      _cts?.Dispose();
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An error occurred stopping worker {wName}.", GetType().Name);
    }
  }
}