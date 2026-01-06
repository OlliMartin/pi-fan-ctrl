using System.Device.Pwm;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model.Settings;

namespace PiFanCtrl.Services.Pwm;

public sealed class GpioPwmController : IPwmController, IDisposable
{
  private const int FAN_PWM_FREQUENCY = 25_000;

  private static readonly double DEFAULT_DUTY_CYCLE = double.TryParse(Environment.GetEnvironmentVariable("DEFAULT_PWM"), out double result)
      ? result
      : 1;

  private readonly ILogger _logger;
  private readonly PwmChannel _pwmChannel;

  public GpioPwmController(
    ILogger<GpioPwmController> logger,
    IOptions<PwmPinConfiguration> configurationOptions
  )
  {
    _logger = logger;

    PwmPinConfiguration config = configurationOptions.Value;

    _logger.LogInformation(
      "Starting pwm channel with chip index {idx} and channel {channel}.",
      config.ChipIndex,
      config.Channel
    );

    try
    {
      _pwmChannel = PwmChannel.Create(
        config.ChipIndex,
        config.Channel,
        FAN_PWM_FREQUENCY,
        DEFAULT_DUTY_CYCLE
      );

      _pwmChannel.Start();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to start PWM controller.");
    }
  }

  public Task SetDutyCycleAsync(decimal percentage, CancellationToken cancelToken = default)
  {
    _pwmChannel.DutyCycle = (double)(percentage / 100m);

    _logger.LogInformation("Updating duty cycle to {newVal}", percentage);
    return Task.CompletedTask;
  }

  public void Dispose()
  {
    Stopwatch sw = Stopwatch.StartNew();
    _logger.LogInformation("Disposing {name}.", nameof(GpioPwmController));

    _pwmChannel.Stop();
    _pwmChannel.Dispose();

    sw.Stop();
    _logger.LogDebug("{name} disposed in {elapsed}.", nameof(GpioPwmController), sw.Elapsed);
  }
}