using System.Device.Gpio;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;
using PiFanCtrl.Model.Settings;

namespace PiFanCtrl.Services.FanRpm;

public sealed class GpioFanRpmSensor : IFanRpmSensor, IDisposable
{
  public string Name => $"Input-{_pinOptions.Value.Pin}";

  private readonly GpioController _gpioController;
  private readonly ILogger _logger;
  private readonly IOptions<FanRpmPinConfiguration> _pinOptions;

  private readonly Stopwatch _lastFallingEdge = Stopwatch.StartNew();
  private double _lastReading = 0;

  public GpioFanRpmSensor(ILogger<GpioFanRpmSensor> logger, IOptions<FanRpmPinConfiguration> pinOptions)
  {
    _logger = logger;
    _pinOptions = pinOptions;

    var pinSettings = pinOptions.Value;

    _gpioController = new();
    _gpioController.OpenPin(pinSettings.Pin, PinMode.InputPullUp);
    _gpioController.RegisterCallbackForPinValueChangedEvent(pinSettings.Pin, PinEventTypes.Falling,
      OnSignalPinFallEvent);
  }

  public Task<FanRpmReading?> ReadNextValueAsync(CancellationToken cancelToken = default)
  {
    return Task.FromResult<FanRpmReading?>(new()
    {
      Sensor = Name,
      Value = (decimal)_lastReading
    });
  }

  private int _preventSpam = 0;

  private void OnSignalPinFallEvent(object sender, PinValueChangedEventArgs args)
  {
    var elapsed = _lastFallingEdge.Elapsed;
    _lastFallingEdge.Restart();

    var freq = 1_000_000_000 / elapsed.TotalNanoseconds;
    _lastReading = (freq / 2) * 60;

    if (_preventSpam++ % 100 == 0)
    {
      _logger.LogInformation("[{time:O}] Calculated RPM {rpm} (Elapsed={elapsed}).", DateTime.Now,
        _lastReading, elapsed);
    }
  }

  public void Dispose()
  {
    _gpioController.Dispose();
  }
}