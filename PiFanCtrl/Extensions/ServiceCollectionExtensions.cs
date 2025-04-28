using Microsoft.Extensions.Options;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model.Settings;
using PiFanCtrl.Services.Temperature;

namespace PiFanCtrl.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddUnifiServices(
    this IServiceCollection services,
    IConfiguration configuration,
    UnifiSensorsConfiguration settings
  )
  {
    services
      .AddTransient<UnifiAuthenticationMessageHandler>()
      .AddHttpClient<UnifiTemperatureSensor>(
        (sp, client) =>
        {
          IOptions<UnifiSensorsConfiguration> options =
            sp.GetRequiredService<IOptions<UnifiSensorsConfiguration>>();

          UnifiSensorsConfiguration resolvedSettings = options.Value;

          client.BaseAddress = resolvedSettings.BaseUri;
        }
      )
      .AddHttpMessageHandler<UnifiAuthenticationMessageHandler>();

    return services
      .Configure<UnifiSensorsConfiguration>(configuration)
      .AddSingleton<ITemperatureSensor>(
        sp =>
        {
          ILogger<UnifiTemperatureSensor> logger = sp.GetRequiredService<ILogger<UnifiTemperatureSensor>>();

          IOptions<UnifiSensorsConfiguration> cfgOptions =
            sp.GetRequiredService<IOptions<UnifiSensorsConfiguration>>();

          IHttpClientFactory hcf = sp.GetRequiredService<IHttpClientFactory>();

          UnifiTemperatureSensor unifiApi = new(logger, cfgOptions, hcf);

          return unifiApi;
        }
      );
  }
}