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
  private const int fontSize = 25;
  private const string font = "DejaVu Sans";

  private readonly ILogger<SystemInfoWorker> _logger;

  private readonly I2cDevice _i2cDevice;
  private readonly Ssd1306 _device;

  private readonly PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(milliseconds: 200));
  private CancellationTokenSource? _cts;

  public SystemInfoWorker(ILogger<SystemInfoWorker> logger)
  {
    _logger = logger;

    I2cConnectionSettings connectionSettings = new(busId: 1, deviceAddress: 0x3C);
    _i2cDevice = I2cDevice.Create(connectionSettings);
    _device = new(_i2cDevice, width: 128, height: 32);
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

  private async Task RunTimerAsync(CancellationToken cancelToken = default)
  {
    try
    {
      while (await _timer.WaitForNextTickAsync(cancelToken))
      {
        using BitmapImage image = BitmapImage.CreateBitmap(
          width: 128,
          height: 32,
          PixelFormat.Format32bppArgb
        );

        int y = 0;
        // image.Clear(Color.Black);

        IGraphics g = image.GetDrawingApi();

        g.DrawText(DateTime.Now.ToString("HH:mm:ss"), font, fontSize, Color.White, new(x: 0, y));

        _device.DrawBitmap(image);
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

      _device.Dispose();
      _i2cDevice.Dispose();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An error occurred stopping worker {wName}.", GetType().Name);
    }
  }
}