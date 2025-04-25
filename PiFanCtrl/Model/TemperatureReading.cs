namespace PiFanCtrl.Model;

public record TemperatureReading
{
  public required string Sensor { get; init; }
  
  public bool IsOverride { get; init; } = false;
  
  public decimal Value { get; init; }

  public DateTime AsOf { get; init; } = DateTime.UtcNow;
}