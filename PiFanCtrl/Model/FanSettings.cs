namespace PiFanCtrl.Model;

public record FanSettings
{
  public decimal MinimumSpeedTemperature { get; set; }

  public decimal MinimumSpeed { get; set; }

  public decimal PanicFromTemperature { get; set; }

  public decimal FallbackTemperature { get; set; } = 60m;

  public decimal PanicSpeed { get; set; }

  public IList<CurvePoint> CurvePoints { get; init; } = [];

  public FanSettings AddPoint(CurvePoint pointToAdd)
  {
    List<CurvePoint> newList = CurvePoints.Select(cp => cp with { }).ToList();
    newList.Add(pointToAdd);

    return this with
    {
      CurvePoints = newList.OrderBy(cp => cp.Temperature).ToList(),
    };
  }

  public FanSettings RemovePoint(CurvePoint pointToRemove)
  {
    return this with
    {
      CurvePoints = CurvePoints.Where(p => p.Guid != pointToRemove.Guid).OrderBy(cp => cp.Temperature)
        .ToList(),
    };
  }
}