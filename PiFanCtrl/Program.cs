using System.Runtime.InteropServices;
using PiFanCtrl.Components;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model.Settings;
using PiFanCtrl.Services;
using PiFanCtrl.Services.FanRpm;
using PiFanCtrl.Services.Pwm;
using PiFanCtrl.Services.Stores;
using PiFanCtrl.Services.Temperature;
using PiFanCtrl.Workers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
  .AddInteractiveServerComponents();

builder.Services.AddLogging(opts =>
{
  opts.SetMinimumLevel(LogLevel.Debug);
  opts.AddConsole();
});

var rootConfiguration = builder.Configuration.GetSection(RootSettings.SECTION_NAME);

var pwmConfiguration = rootConfiguration.GetSection(PwmPinConfiguration.SECTION_NAME);
var temperatureConfiguration = rootConfiguration.GetSection(TemperatureSensorConfiguration.SECTION_NAME);
var fanRpmConfiguration = rootConfiguration.GetSection(FanRpmPinConfiguration.SECTION_NAME);

builder.Services.Configure<RootSettings>(rootConfiguration)
  .Configure<PwmPinConfiguration>(pwmConfiguration)
  .Configure<TemperatureSensorConfiguration>(temperatureConfiguration)
  .Configure<FanRpmPinConfiguration>(fanRpmConfiguration);

builder.Services
  .AddSingleton<ITemperatureStore, SlidingTemperatureStore>()
  .AddSingleton<ITemperatureSensor, DummyTemperatureSensor>()
  .AddSingleton<DummyTemperatureSensor>(sp =>
    (sp.GetServices<ITemperatureSensor>()
      .Single(ts => ts is DummyTemperatureSensor) as DummyTemperatureSensor)!)
  .AddKeyedSingleton<ITemperatureSensor, TemperatureWrapper>("delegating")
  .AddSingleton<PwmControllerWrapper>();

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

foreach (var sensorDef in temperatureConfiguration.Get<TemperatureSensorConfiguration>()?.Sensors ?? [])
{
  if (sensorDef.Type == TemperatureSensor.DHT22)
  {
    builder.Services.AddSingleton<ITemperatureSensor>(sp =>
    {
      var logger = sp.GetRequiredService<ILogger<DHT22TemperatureSensor>>();
      var sensor = new DHT22TemperatureSensor(logger, sensorDef);
      return sensor;
    });
  }
  else
  {
    throw new InvalidOperationException($"Unhandled/unknown sensor type {sensorDef.Type}. Cannot start.");
  }
}

var app = builder.Build();

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