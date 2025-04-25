using PiFanCtrl.Components;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Services;
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

builder.Services
  .AddSingleton<ITemperatureStore, SlidingTemperatureStore>()
  .AddSingleton<ITemperatureSensor, DummyTemperatureSensor>()
  .AddSingleton<DummyTemperatureSensor>(sp =>
    (sp.GetServices<ITemperatureSensor>()
      .Single(ts => ts is DummyTemperatureSensor) as DummyTemperatureSensor)!)
  .AddKeyedSingleton<ITemperatureSensor, TemperatureWrapper>("delegating")
  .AddSingleton<PwmControllerWrapper>();

if (false)
{
  builder.Services.AddSingleton<IPwmController, DummyPwmController>();
}
else
{
  builder.Services.AddSingleton<IPwmController, GpioPwmController>();
}

builder.Services.AddHostedService<PwmControlWorker>();

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