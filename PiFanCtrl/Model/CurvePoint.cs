namespace PiFanCtrl.Model;

public record CurvePoint
{
  public Guid Guid { get; init; } = Guid.NewGuid();

  public bool Active { get; set; }

  public decimal Temperature { get; init; }

  public decimal FanPercentage { get; init; }
}

public record CurvePointUi
{
  public bool Active { get; set; }

  public decimal? Temperature { get; set; }

  public decimal? FanPercentage { get; set; }

  public CurvePoint ToPoint() => new()
  {
    Active = Temperature is not null && FanPercentage is not null,
    Temperature = Temperature ?? 0,
    FanPercentage = FanPercentage ?? 0,
  };
}