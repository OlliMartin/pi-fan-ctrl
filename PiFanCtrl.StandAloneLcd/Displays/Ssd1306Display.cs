using System.Device.I2c;
using System.Drawing;
using Iot.Device.Graphics;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Iot.Device.Ssd13xx;
using Microsoft.Extensions.Logging;
using PiFanControl.Abstractions;
using PiFanCtrl.StandAloneLcd.Interfaces;
using SkiaSharp;

namespace PiFanCtrl.StandAloneLcd.Displays;

public sealed class Ssd1306Display : IDisplay, IDisposable
{
  private readonly ILogger<Ssd1306Display> _logger;
  private const int REFRESH_INTERVAL_IN_S = 300;

  private static TimeSpan _renewAfter = TimeSpan.FromSeconds(REFRESH_INTERVAL_IN_S);
  private DateTime _lastRenew;

  private const int fontSize = 14;
  private const string font = "DejaVu Mono";
  
  private I2cDevice? _i2cDevice;
  private Ssd1306? _device;

  private bool printAggregateTemp = true;

  public Ssd1306Display(ILogger<Ssd1306Display> logger)
  {
    _logger = logger;
  }
  
  public Task Draw(SystemInfo systemInfo, CancellationToken cancelToken = default)
  {
    using BitmapImage image = BitmapImage.CreateBitmap(
      width: 128,
      height: 32,
      PixelFormat.Format32bppArgb
    );

    IGraphics g = image.GetDrawingApi();

    printAggregateTemp = !printAggregateTemp;

    if (printAggregateTemp)
    {
      g.DrawText($"A-Temp:", font, fontSize, Color.White, new(x: 0, y: 0));
      g.DrawText($"{systemInfo.AggregatedTemperature:F2}", font, fontSize, Color.White, new(x: 128 - 3*fontSize, y: 0));
    }
    else
    {
      g.DrawText($"M-Temp:", font, fontSize, Color.White, new(x: 0, y: 0));
      g.DrawText($"{systemInfo.MeasuredTemperature:F2}", font, fontSize, Color.White, new(x: 128 - 3*fontSize, y: 0));
    }
    
    g.DrawText($"Fan%:", font, fontSize, Color.White, new(x: 0, y: 16));
    g.DrawText($"{systemInfo.PwmPercentage:F2}", font, fontSize, Color.White, new(x: 128- 3*fontSize, y: 16));

    Ssd1306 device = GetOrRenewDevice();
    device.DrawBitmap(image);
    
    return Task.CompletedTask;
  }
  
  private Ssd1306 GetOrRenewDevice()
  {
    if (_device is null || _lastRenew + _renewAfter < DateTime.UtcNow)
    {
      _logger.LogDebug("Creating or renewing device.");

      I2cConnectionSettings connectionSettings = new(busId: 1, deviceAddress: 0x3C);
      I2cDevice i2cDevice = I2cDevice.Create(connectionSettings);
      Ssd1306 device = new(i2cDevice, width: 128, height: 32);

      I2cDevice? oldI2c = Interlocked.Exchange(ref _i2cDevice, i2cDevice);
      Ssd1306? oldDev = Interlocked.Exchange(ref _device, device);

      oldI2c?.Dispose();
      oldDev?.Dispose();

      _lastRenew = DateTime.UtcNow;
    }

    return _device;
  }

  public void Dispose()
  {
    _device?.Dispose();
    _i2cDevice?.Dispose();
  }
}