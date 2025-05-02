using Iot.Device.Graphics.SkiaSharpAdapter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PiFanCtrl.StandAloneLcd.Workers;

SkiaSharpAdapter.Register();

IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);

hostBuilder.UseSystemd();

hostBuilder.ConfigureLogging(
  opts =>
  {
    opts.SetMinimumLevel(LogLevel.Trace);
    opts.AddConsole();
  }
);

hostBuilder.ConfigureServices(
  (ctx, sc) => { sc.AddHostedService<SystemInfoWorker>(); }
);

IHost host = hostBuilder.Build();
host.Run();