using System.Device.Gpio;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;
using PiFanCtrl.Model.Settings;

namespace PiFanCtrl.Services.FanRpm;

public sealed class GpioFanRpmSensor : IFanRpmSensor, IDisposable
{
  public string Name => $"RPM-Pin-{_pinOptions.Value.Pin}";

  private readonly GpioController _gpioController;
  private readonly ILogger _logger;
  private readonly IOptions<FanRpmPinConfiguration> _pinOptions;

  private readonly Stopwatch _lastMeasurement = Stopwatch.StartNew();

  public GpioFanRpmSensor(ILogger<GpioFanRpmSensor> logger, IOptions<FanRpmPinConfiguration> pinOptions)
  {
    _logger = logger;
    _pinOptions = pinOptions;

    FanRpmPinConfiguration pinSettings = pinOptions.Value;

    _logger.LogInformation("Starting fan rpm sensor on pin {pin}.", pinSettings.Pin);

    _gpioController = new();
    _gpioController.OpenPin(pinSettings.Pin, PinMode.InputPullUp);

    _gpioController.RegisterCallbackForPinValueChangedEvent(
      pinSettings.Pin,
      PinEventTypes.Falling,
      OnSignalPinFallEvent
    );
  }

  public Task<FanRpmReading?> ReadNextValueAsync(CancellationToken cancelToken = default)
  {
    int interruptCount = Interlocked.Exchange(ref _interruptsSinceLastMeasurement, value: 0);
    TimeSpan duration = _lastMeasurement.Elapsed;
    _lastMeasurement.Restart();

    decimal rpm = interruptCount / 2m * (decimal)TimeSpan.FromMinutes(minutes: 1).TotalNanoseconds /
                  (decimal)duration.TotalNanoseconds;

    _logger.LogInformation(
      "{cnt} interrupts occurred in {duration}, RPM is: {rpm}",
      interruptCount,
      duration,
      rpm
    );

    return Task.FromResult<FanRpmReading?>(
      new()
      {
        Source = Name,
        Value = rpm,
      }
    );
  }

  private int _interruptsSinceLastMeasurement = 0;

  private void OnSignalPinFallEvent(object sender, PinValueChangedEventArgs args)
  {
    Interlocked.Increment(ref _interruptsSinceLastMeasurement);
  }

  public void Dispose()
  {
    Stopwatch sw = Stopwatch.StartNew();
    _logger.LogInformation("Disposing {name}.", nameof(GpioFanRpmSensor));

    FanRpmPinConfiguration pinSettings = _pinOptions.Value;
    _gpioController.UnregisterCallbackForPinValueChangedEvent(pinSettings.Pin, OnSignalPinFallEvent);
    _gpioController.Dispose();

    sw.Stop();
    _logger.LogDebug("{name} disposed in {elapsed}.", nameof(GpioFanRpmSensor), sw.Elapsed);
  }
}