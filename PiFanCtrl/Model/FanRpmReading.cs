namespace PiFanCtrl.Model;

public record FanRpmReading
{
  public required string Sensor { get; init; }
  
  public decimal Value { get; init; }
  
  public DateTime AsOf { get; init; } = DateTime.UtcNow;
}