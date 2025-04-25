using PiFanCtrl.Interfaces;
using PiFanCtrl.Services.Pwm;

namespace PiFanCtrl.Workers;

public class PwmControlWorker(
  [FromKeyedServices("delegating")] ITemperatureSensor temperatureSensor,
  PwmControllerWrapper pwmController,
  ITemperatureStore temperatureStore,
  ILogger<PwmControlWorker> logger
) : IHostedService
{
  private const decimal DEFAULT_DUTY_CYCLE = 100;
  private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(5));

  private CancellationTokenSource? _cts;
  
  public async Task StartAsync(CancellationToken cancellationToken)
  {
    logger.LogInformation("Starting pwm control worker.");
    
    _cts = new CancellationTokenSource();
    _ = RunTimerAsync(_cts.Token);

    logger.LogInformation("Initializing duty cycle to {val}.", DEFAULT_DUTY_CYCLE);
    await pwmController.SetDutyCycleAsync(DEFAULT_DUTY_CYCLE, cancellationToken);
  }

  private async Task RunTimerAsync(CancellationToken cancelToken = default)
  {
    try
    {
      while (await _timer.WaitForNextTickAsync(cancelToken))
      {
        var reading = await temperatureSensor.ReadNextValueAsync(cancelToken);
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