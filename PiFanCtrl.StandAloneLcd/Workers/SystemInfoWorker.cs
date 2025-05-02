using System.Device.I2c;
using System.Diagnostics;
using System.Drawing;
using Iot.Device.Graphics;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Iot.Device.Ssd13xx;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PiFanCtrl.StandAloneLcd.Workers;

public class SystemInfoWorker : IHostedService
{
  private static TimeSpan _renewAfter = TimeSpan.FromSeconds(seconds: 60);
  private DateTime _lastRenew;

  private const int fontSize = 12;
  private const string font = "DejaVu Sans";

  private readonly ILogger<SystemInfoWorker> _logger;

  private I2cDevice? _i2cDevice;
  private Ssd1306? _device;

  private readonly PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(milliseconds: 100));
  private CancellationTokenSource? _cts;

  public SystemInfoWorker(ILogger<SystemInfoWorker> logger)
  {
    _logger = logger;

    I2cConnectionSettings connectionSettings = new(busId: 1, deviceAddress: 0x3C);
    // _i2cDevice = I2cDevice.Create(connectionSettings);
    // _device = new(_i2cDevice, width: 128, height: 32);
  }

  public Task StartAsync(CancellationToken cancellationToken)
  {
    Stopwatch swStart = Stopwatch.StartNew();
    _logger.LogInformation("Starting sys info worker.");

    _cts = new();
    _ = Task.Run(() => RunTimerAsync(_cts.Token), cancellationToken);

    swStart.Stop();
    _logger.LogInformation("{wName} started in {elapsed}.", nameof(SystemInfoWorker), swStart.Elapsed);
    return Task.CompletedTask;
  }

  private int i = 0;

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

  private int y = 0;

  private async Task RunTimerAsync(CancellationToken cancelToken = default)
  {
    try
    {
      while (await _timer.WaitForNextTickAsync(cancelToken))
      {
        // using BitmapImage image = BitmapImage.CreateBitmap(
        //   width: 128,
        //   height: 32,
        //   PixelFormat.Format32bppArgb
        // );
        //
        // IGraphics g = image.GetDrawingApi();
        //
        // g.DrawText(DateTime.UtcNow.ToString("HH:mm:ss zz"), font, fontSize, Color.White, new(x: 0, y: 0));
        //
        // Ssd1306 device = GetOrRenewDevice();
        // device.DrawBitmap(image);

        Ssd1306 device = GetOrRenewDevice();

        using BitmapImage image = BitmapImage.CreateBitmap(
          width: 128,
          height: 32,
          PixelFormat.Format32bppArgb
        );

        image.Clear(Color.Black);
        IGraphics g = image.GetDrawingApi();
        g.DrawText(DateTime.Now.ToString("HH:mm:ss"), font, fontSize, Color.White, new(x: 0, y));

        device.DrawBitmap(image);

        y++;

        if (y >= image.Height)
        {
          y = 0;
        }
      }
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Stopped.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An error occurred during sys info processing.");

      throw;
    }
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("{wName} is stopping.", nameof(SystemInfoWorker));

    try
    {
      await (_cts?.CancelAsync() ?? Task.CompletedTask);
      _cts?.Dispose();

      _device?.Dispose();
      _i2cDevice?.Dispose();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An error occurred stopping worker {wName}.", GetType().Name);
    }
  }
}