using PiFanCtrl.Interfaces;

namespace PiFanCtrl.Model;

public record TemperatureReading : IReading
{
  public string Measurement => "Temperature";

  public required string Source { get; init; }

  public bool IsOverride { get; init; } = false;

  public decimal Value { get; init; }

  public bool Active { get; init; } = true;

  public DateTime AsOf { get; init; } = DateTime.UtcNow;

  public Dictionary<string, string> Metadata => new()
  {
    ["IsOverride"] = IsOverride
      ? "true"
      : "false",
  };
}