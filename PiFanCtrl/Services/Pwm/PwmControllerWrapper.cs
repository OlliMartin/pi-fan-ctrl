using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;

namespace PiFanCtrl.Services.Pwm;

public class PwmControllerWrapper(
  IPwmController pwmController,
  [FromKeyedServices("delegating")] IReadingStore readingStore
)
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
      await UpdateDutyCycleAsync(percentage, isOverride: false, cancelToken);
    }

    _lastValue = percentage;
  }

  public async Task OverrideDutyCycleAsync(decimal percentage, CancellationToken cancelToken = default)
  {
    _hasOverride = true;

    await UpdateDutyCycleAsync(percentage, isOverride: true, cancelToken);
  }

  public async Task ResetAsync(CancellationToken cancelToken = default)
  {
    _hasOverride = false;
    await UpdateDutyCycleAsync(_lastValue, isOverride: false, cancelToken);
  }

  private async Task UpdateDutyCycleAsync(decimal percentage, bool isOverride, CancellationToken cancelToken)
  {
    await readingStore.AddAsync(
      new PwmDutyCycleReading()
      {
        Value = percentage,
        Metadata =
        {
          ["IsOverride"] = isOverride
            ? "true"
            : "false",
        },
      },
      cancelToken
    );

    await pwmController.SetDutyCycleAsync(percentage, cancelToken);
  }
}