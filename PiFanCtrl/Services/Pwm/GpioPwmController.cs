using System.Device.Pwm;
using Microsoft.Extensions.Options;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model.Settings;

namespace PiFanCtrl.Services.Pwm;

public sealed class GpioPwmController : IPwmController, IDisposable
{
  private const int FAN_PWM_FREQUENCY = 25_000;
  private const double DEFAULT_DUTY_CYCLE = 100;

  private readonly ILogger _logger;
  private readonly PwmChannel _pwmChannel;

  public GpioPwmController(ILogger<GpioPwmController> logger,
    IOptions<PwmPinConfiguration> configurationOptions)
  {
    _logger = logger;

    PwmPinConfiguration config = configurationOptions.Value;

    _logger.LogInformation("Starting pwm channel with chip index {idx} and channel {channel}.",
      config.ChipIndex, config.Channel);

    _pwmChannel = PwmChannel.Create(config.ChipIndex, config.Channel, FAN_PWM_FREQUENCY, DEFAULT_DUTY_CYCLE);
    _pwmChannel.Start();
  }

  public Task SetDutyCycleAsync(decimal percentage, CancellationToken cancelToken = default)
  {
    _pwmChannel.DutyCycle = (double)percentage;
    
    _logger.LogInformation("Updating duty cycle to {newVal}", percentage);
    return Task.CompletedTask;
  }

  public void Dispose()
  {
    _pwmChannel.Stop();
    _pwmChannel.Dispose();
  }
}