namespace PiFanCtrl.Interfaces;

public interface IPwmController
{
  Task SetDutyCycleAsync(decimal percentage, CancellationToken cancelToken = default);
}