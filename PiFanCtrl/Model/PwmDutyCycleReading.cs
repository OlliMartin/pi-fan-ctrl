using PiFanCtrl.Interfaces;

namespace PiFanCtrl.Model;

public class PwmDutyCycleReading : IReading
{
  public string Measurement => "DutyCycle";
  public string Source { get; init; } = "Calculated";

  public decimal Value { get; init; }
  public DateTime AsOf { get; init; } = DateTime.UtcNow;
  
  public bool Active => true;

  public Dictionary<string, string> Metadata { get; } = new();
}