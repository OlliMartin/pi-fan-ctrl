using PiFanCtrl.Interfaces;

namespace PiFanCtrl.Services.Pwm;

public class DummyPwmController(ILogger<DummyPwmController> logger) : IPwmController
{
  public Task SetDutyCycleAsync(decimal percentage, CancellationToken cancelToken = default)
  {
    logger.LogInformation("Changing duty cycle to {newVal}.", percentage);
    return Task.CompletedTask;
  }
}