using System.Text.Json.Serialization;

namespace PiFanCtrl.Model.Settings;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TemperatureSensor
{
  DHT22,
  Unifi,
  BMP280,
}

public class HardwareSensorConfiguration : SensorConfiguration
{
  public int Pin { get; init; }
}

public class SensorConfiguration
{
  public const string TYPE_SECTION = nameof(Type);

  public TemperatureSensor Type { get; init; }
}

public class TemperatureSensorConfiguration
{
  public const string SECTION_NAME = "Temperature";

  public const string SENSOR_SECTION = nameof(Sensors);
  // TODO

  public IEnumerable<SensorConfiguration> Sensors { get; init; } = new List<SensorConfiguration>();
}