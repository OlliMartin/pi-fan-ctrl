using PiFanCtrl.Interfaces;

namespace PiFanCtrl.Model;

public record FanRpmReading : IReading
{
  public string Measurement => "FanRpm";
  public required string Source { get; init; }

  public decimal Value { get; init; }

  public bool Active => true;

  public DateTime AsOf { get; init; } = DateTime.UtcNow;

  public Dictionary<string, string> Metadata { get; } = new();
}