namespace PiFanControl.Abstractions;

public record SystemInfo
{
  public decimal AggregatedTemperature { get; init; }
  
  public decimal MeasuredTemperature { get; init; }
  
  public decimal PwmPercentage { get; init; }
  
  public decimal MeasuredFanRpm { get; init; }
  
  public DateTime AsOf { get; init; }
}