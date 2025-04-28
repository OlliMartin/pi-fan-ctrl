using System.Runtime.InteropServices;
using PiFanCtrl.Components;
using PiFanCtrl.Factories;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model.Settings;
using PiFanCtrl.Services;
using PiFanCtrl.Services.FanRpm;
using PiFanCtrl.Services.Pwm;
using PiFanCtrl.Services.Stores;
using PiFanCtrl.Services.Temperature;
using PiFanCtrl.Workers;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
  .AddInteractiveServerComponents();

builder.Services.AddLogging(
  opts =>
  {
    opts.SetMinimumLevel(LogLevel.Debug);
    opts.AddConsole();
  }
);

IConfigurationSection rootConfiguration = builder.Configuration.GetSection(RootSettings.SECTION_NAME);

IConfigurationSection pwmConfiguration = rootConfiguration.GetSection(PwmPinConfiguration.SECTION_NAME);

IConfigurationSection temperatureConfiguration =
  rootConfiguration.GetSection(TemperatureSensorConfiguration.SECTION_NAME);

IConfigurationSection fanRpmConfiguration = rootConfiguration.GetSection(FanRpmPinConfiguration.SECTION_NAME);

IConfigurationSection influxConfiguration = rootConfiguration.GetSection(InfluxConfiguration.SECTION_NAME);

builder.Services.Configure<RootSettings>(rootConfiguration)
  .Configure<PwmPinConfiguration>(pwmConfiguration)
  .Configure<TemperatureSensorConfiguration>(temperatureConfiguration)
  .Configure<FanRpmPinConfiguration>(fanRpmConfiguration)
  .Configure<InfluxConfiguration>(influxConfiguration);

builder.Services
  .AddSingleton<SlidingReadingStore>()
  .AddSingleton<IReadingStore>(sp => sp.GetRequiredService<SlidingReadingStore>())
  .AddSingleton<ITemperatureSensor, DummyTemperatureSensor>()
  .AddKeyedSingleton<IReadingStore, ReadingStoreWrapper>("delegating")
  .AddSingleton<DummyTemperatureSensor>(
    sp =>
      (sp.GetServices<ITemperatureSensor>()
        .Single(ts => ts is DummyTemperatureSensor) as DummyTemperatureSensor)!
  )
  .AddKeyedSingleton<ITemperatureSensor, TemperatureWrapper>("delegating")
  .AddSingleton<PwmControllerWrapper>();

if (influxConfiguration.Exists())
{
  builder.Services.AddSingleton<IReadingStore, InfluxReadingStore>();
}

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
  builder.Services.AddSingleton<IPwmController, DummyPwmController>();
}
else
{
  builder.Services.AddSingleton<IPwmController, GpioPwmController>();
}

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
  builder.Services.AddSingleton<IFanRpmSensor, DummyFanRpmSensor>();
}
else
{
  builder.Services.AddSingleton<IFanRpmSensor, GpioFanRpmSensor>();
}

builder.Services.AddHostedService<PwmControlWorker>();
builder.Services.AddHostedService<FanRpmWorker>();

SensorFactory.RegisterSensorServices(builder.Services, temperatureConfiguration);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error", createScopeForErrors: true);
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
  .AddInteractiveServerRenderMode();

app.Run();