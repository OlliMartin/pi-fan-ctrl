using System.Device.I2c;
using System.Diagnostics;
using System.Drawing;
using Iot.Device.Graphics;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Iot.Device.Ssd13xx;
using Iot.Device.Ssd13xx.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PiFanControl.Abstractions;
using PiFanCtrl.StandAloneLcd.Interfaces;

namespace PiFanCtrl.StandAloneLcd.Workers;

public class SystemInfoWorker : IHostedService
{
  private readonly ILogger<SystemInfoWorker> _logger;
  private readonly IDisplay _display;

  private readonly PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(milliseconds: 2500));
  private CancellationTokenSource? _cts;

  public SystemInfoWorker(ILogger<SystemInfoWorker> logger, IDisplay display)
  {
    _logger = logger;
    _display = display;
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
        SystemInfo sysInfo = new();
        await _display.Draw(sysInfo, cancelToken);
      }
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Stopped.");
    }
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("{wName} is stopping.", nameof(SystemInfoWorker));

    try
    {
      await (_cts?.CancelAsync() ?? Task.CompletedTask);
      _cts?.Dispose();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An error occurred stopping worker {wName}.", GetType().Name);
    }
  }
}