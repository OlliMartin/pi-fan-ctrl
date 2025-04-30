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
    IConfigurationSection sensorConfiguration =
      sensorRootConfiguration.GetSection(TemperatureSensorConfiguration.SENSOR_SECTION);

    foreach (IConfigurationSection sensorDef in sensorConfiguration.GetChildren())
    {
      SensorConfiguration settingsInstance = GetSettingsInstance(sensorDef);
      RegisterServiceForSetting(serviceCollection, sensorDef, settingsInstance);
    }
  }

  private static SensorConfiguration GetSettingsInstance(IConfiguration sensorConfiguration)
  {
    TemperatureSensor type =
      sensorConfiguration.GetValue<TemperatureSensor>(SensorConfiguration.TYPE_SECTION);

    SensorConfiguration? result = type switch
    {
      TemperatureSensor.Unifi => sensorConfiguration.Get<UnifiSensorsConfiguration>(),
      TemperatureSensor.DHT22 => sensorConfiguration.Get<HardwareSensorConfiguration>(),
      TemperatureSensor.BMP280 => sensorConfiguration.Get<I2CSensorConfiguration>(),
      var _ => throw new InvalidOperationException(
        $"Unknown sensor type {type}. This is a configuration error."
      ),
    };

    if (result is null)
    {
      throw new InvalidOperationException(
        $"Could not parse sensor settings for type {type}. Please check the configuration."
      );
    }

    return result;
  }

  private static void RegisterServiceForSetting(
    IServiceCollection serviceCollection,
    IConfiguration configurationSection,
    SensorConfiguration sensorConfiguration
  )
  {
    switch (sensorConfiguration)
    {
      case I2CSensorConfiguration i2cCfg:
        RegisterI2CService(serviceCollection, i2cCfg);
        break;
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

  private static void RegisterI2CService(
    IServiceCollection serviceCollection,
    I2CSensorConfiguration i2cCfg
  )
  {
    if (i2cCfg.Type == TemperatureSensor.BMP280)
    {
      serviceCollection.AddSingleton<ITemperatureSensor>(
        sp =>
        {
          ILogger<Bmp280TemperatureSensor> logger = sp.GetRequiredService<ILogger<Bmp280TemperatureSensor>>();
          Bmp280TemperatureSensor sensor = new(logger, i2cCfg);
          return sensor;
        }
      );
    }
    else
    {
      throw new InvalidOperationException($"Unhandled/unknown sensor type {i2cCfg.Type}. Cannot start.");
    }
  }

  private static void RegisterHardwareService(
    IServiceCollection serviceCollection,
    HardwareSensorConfiguration hardwareCfg
  )
  {
    if (hardwareCfg.Type == TemperatureSensor.DHT22)
    {
      serviceCollection.AddSingleton<ITemperatureSensor>(
        sp =>
        {
          ILogger<DHT22TemperatureSensor> logger = sp.GetRequiredService<ILogger<DHT22TemperatureSensor>>();
          DHT22TemperatureSensor sensor = new(logger, hardwareCfg);
          return sensor;
        }
      );
    }
    else
    {
      throw new InvalidOperationException($"Unhandled/unknown sensor type {hardwareCfg.Type}. Cannot start.");
    }
  }
}