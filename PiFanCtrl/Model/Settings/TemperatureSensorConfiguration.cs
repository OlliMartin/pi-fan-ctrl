using System.Text.Json.Serialization;

namespace PiFanCtrl.Model.Settings;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TemperatureSensor
{
  DHT22
}

public class SensorConfiguration
{
  public TemperatureSensor Type { get; init; }
  
  public int Pin { get; init; }
}

public class TemperatureSensorConfiguration
{
  public const string SECTION_NAME = "Temperature";
  // TODO

  public IEnumerable<SensorConfiguration> Sensors { get; init; } = new List<SensorConfiguration>();
}