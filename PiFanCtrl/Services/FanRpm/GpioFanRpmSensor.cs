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

  private readonly Stopwatch _lastMeasurement = Stopwatch.StartNew();
  
  public GpioFanRpmSensor(ILogger<GpioFanRpmSensor> logger, IOptions<FanRpmPinConfiguration> pinOptions)
  {
    _logger = logger;
    _pinOptions = pinOptions;

    var pinSettings = pinOptions.Value;

    _logger.LogInformation("Starting fan rpm sensor on pin {pin}.", pinSettings.Pin);
    
    _gpioController = new();
    _gpioController.OpenPin(pinSettings.Pin, PinMode.InputPullUp);
    _gpioController.RegisterCallbackForPinValueChangedEvent(pinSettings.Pin, PinEventTypes.Falling,
      OnSignalPinFallEvent);
  }

  public Task<FanRpmReading?> ReadNextValueAsync(CancellationToken cancelToken = default)
  {
    var interruptCount = Interlocked.Exchange(ref _interruptsSinceLastMeasurement, 0);
    var duration = _lastMeasurement.Elapsed;
    _lastMeasurement.Restart();
    
    var rpm = (interruptCount / 2m * (decimal)TimeSpan.FromMinutes(1).TotalNanoseconds) / ((decimal)duration.TotalNanoseconds);
    
    _logger.LogInformation("{cnt} interrupts occurred in {duration}, RPM is: {rpm}", interruptCount, duration, rpm);
    
    return Task.FromResult<FanRpmReading?>(new()
    {
      Sensor = Name,
      Value = rpm
    });
  }
  
  private int _interruptsSinceLastMeasurement = 0;
  private void OnSignalPinFallEvent(object sender, PinValueChangedEventArgs args)
  {
    Interlocked.Increment(ref _interruptsSinceLastMeasurement);
  }

  public void Dispose()
  {
    _gpioController.Dispose();
  }
}