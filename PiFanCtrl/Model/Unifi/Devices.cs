using System.Diagnostics;
using System.Text.Json.Serialization;

namespace PiFanCtrl.Model.Unifi;

public class TemperatureItem
{
  [JsonPropertyName("name")]
  public string Name { get; set; }

  [JsonPropertyName("type")]
  public string Type { get; set; }

  [JsonPropertyName("value")]
  public float Value { get; set; }
}

public class SystemStats
{
  [JsonPropertyName("cpu")]
  public float Cpu { get; init; }

  [JsonPropertyName("mem")]
  public float Mem { get; init; }

  [JsonPropertyName("uptime")]
  public long Uptime { get; init; }
}

[DebuggerDisplay("{Name}")]
public class DeviceData
{
  [JsonPropertyName("name")]
  public string Name { get; init; }

  [JsonPropertyName("shortname")]
  public string ShortName { get; init; }

  [JsonPropertyName("guid")]
  public string Guid { get; init; }

  [JsonPropertyName("system-stats")]
  public SystemStats Stats { get; init; }

  [JsonPropertyName("has_temperature")]
  public bool HasTemperature { get; init; }

  [JsonPropertyName("temperatures")]
  public IReadOnlyList<TemperatureItem> Temperatures { get; init; } = [];

  [JsonPropertyName("general_temperature")]
  public float? GeneralTemperature { get; init; }

  public decimal GetTemperature() =>
    // TODO: This function is wrong; Figure out how to retrieve data.
    (decimal)(GeneralTemperature ?? Temperatures.FirstOrDefault()?.Value ?? -1f);

  public bool CanGetTemperature() =>
    (HasTemperature && GeneralTemperature is not null) || Temperatures.Count > 0;
}

public class DeviceResponse
{
  [JsonPropertyName("data")]
  public IList<DeviceData> Data { get; init; } = [];
}