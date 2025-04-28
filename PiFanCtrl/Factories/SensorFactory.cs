using PiFanCtrl.Extensions;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model.Settings;
using PiFanCtrl.Services.Temperature;

namespace PiFanCtrl.Factories;

public static class SensorFactory
{
  public static void RegisterSensorServices(
    IServiceCollection serviceCollection,
    IConfiguration sensorRootConfiguration
  )
  {
    var sensorConfiguration =
      sensorRootConfiguration.GetSection(TemperatureSensorConfiguration.SENSOR_SECTION);

    foreach (var sensorDef in sensorConfiguration.GetChildren())
    {
      var settingsInstance = GetSettingsInstance(sensorDef);
      RegisterServiceForSetting(serviceCollection, sensorDef, settingsInstance);
    }
  }

  private static SensorConfiguration GetSettingsInstance(IConfiguration sensorConfiguration)
  {
    var type = sensorConfiguration.GetValue<TemperatureSensor>(SensorConfiguration.TYPE_SECTION);

    SensorConfiguration? result = type switch
    {
      TemperatureSensor.Unifi => sensorConfiguration.Get<UnifiSensorsConfiguration>(),
      TemperatureSensor.DHT22 => sensorConfiguration.Get<HardwareSensorConfiguration>(),
      _ => throw new InvalidOperationException($"Unknown sensor type {type}. This is a configuration error.")
    };

    if (result is null)
    {
      throw new InvalidOperationException(
        $"Could not parse sensor settings for type {type}. Please check the configuration."
      );
    }

    return result;
  }

  private static void RegisterServiceForSetting(IServiceCollection serviceCollection, IConfiguration configurationSection, SensorConfiguration sensorConfiguration)
  {
    switch (sensorConfiguration)
    {
      case HardwareSensorConfiguration hardwareCfg:
        RegisterHardwareService(serviceCollection, hardwareCfg);
        break;
      case UnifiSensorsConfiguration unifiCfg:
        serviceCollection.AddUnifiServices(configurationSection, unifiCfg);
        break;
      default:
        throw new InvalidOperationException(
          $"Unknown sensor configuration type {sensorConfiguration.GetType().FullName}. This is a programming error."
        );
    }
  }

  private static void RegisterHardwareService(
    IServiceCollection serviceCollection,
    HardwareSensorConfiguration hardwareCfg
  )
  {
    if (hardwareCfg.Type == TemperatureSensor.DHT22)
    {
      serviceCollection.AddSingleton<ITemperatureSensor>(sp =>
      {
        var logger = sp.GetRequiredService<ILogger<DHT22TemperatureSensor>>();
        var sensor = new DHT22TemperatureSensor(logger, hardwareCfg);
        return sensor;
      });
    }
    else
    {
      throw new InvalidOperationException($"Unhandled/unknown sensor type {hardwareCfg.Type}. Cannot start.");
    }   
  }
}