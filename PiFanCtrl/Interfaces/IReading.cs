namespace PiFanCtrl.Interfaces;

public interface IReading
{
  public string Measurement { get; }

  public string Source { get; init; }

  public decimal Value { get; init; }

  public DateTime AsOf { get; init; }

  public Dictionary<string, string> Metadata { get; }
}