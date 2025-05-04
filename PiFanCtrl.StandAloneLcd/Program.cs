using System.Runtime.InteropServices;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PiFanCtrl.StandAloneLcd.Displays;
using PiFanCtrl.StandAloneLcd.Interfaces;
using PiFanCtrl.StandAloneLcd.Services;
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
  (ctx, sc) =>
  {
    sc.AddHostedService<SystemInfoWorker>();
    sc.AddSingleton<ISystemInfoService, CalloutSystemInfoService>();

    sc.AddHttpClient<CalloutSystemInfoService>(
      (client) => { client.BaseAddress = new("https://utils.acaad.dev"); }
    );

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      sc.AddSingleton<IDisplay, DummyDisplay>();
    }
    else
    {
      sc.AddSingleton<IDisplay, Ssd1306Display>();
    }
  }
);


IHost host = hostBuilder.Build();
host.Run();