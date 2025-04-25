using PiFanCtrl.Interfaces;

namespace PiFanCtrl.Services.Pwm;

public class PwmControllerWrapper(IPwmController pwmController)
{
  private const decimal DELTA = 0.00001m; 
  private decimal _lastValue = 0;

  private bool _hasOverride = false;
  
  public async Task SetDutyCycleAsync(decimal percentage, CancellationToken cancelToken = default)
  {
    if (percentage is < 0 or > 100)
    {
      throw new InvalidOperationException("Value for duty cycle must be in interval [0, 100] (inclusive).");
    }
    
    if (!_hasOverride && Math.Abs(percentage - _lastValue) > DELTA)
    {
      await pwmController.SetDutyCycleAsync(percentage, cancelToken);
    }

    _lastValue = percentage;
  }

  public async Task OverrideDutyCycleAsync(decimal percentage, CancellationToken cancelToken = default)
  {
    _hasOverride = true;

    await pwmController.SetDutyCycleAsync(percentage, cancelToken);
  }

  public async Task ResetAsync(CancellationToken cancelToken = default)
  {
    _hasOverride = false;
    await pwmController.SetDutyCycleAsync(_lastValue, cancelToken);
  }
}