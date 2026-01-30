using System.Diagnostics.CodeAnalysis;
using MathNet.Numerics;
using MathNet.Numerics.LinearRegression;
using Microsoft.Extensions.Options;
using PiFanCtrl.Model;

namespace PiFanCtrl.Services;

public class FanSpeedCalculator
{
  private Func<double, double> _curveGenerator;

  private FanSettings _fanSettings;
  private readonly FanSettings _originalFanSettings;

  public FanSettings FanSettings => _fanSettings with { };

  public FanSpeedCalculator(IOptions<FanSettings> fanSettingsOptions)
  {
    _fanSettings = fanSettingsOptions.Value;
    _originalFanSettings = _fanSettings with { };

    ConstructCurveGenerator();
  }

  [MemberNotNull(nameof(_curveGenerator))]
  private void ConstructCurveGenerator()
  {
    List<CurvePoint> activePoints = _fanSettings.CurvePoints.Where(cp => cp.Active).ToList();

    List<double> xData = [(double)_fanSettings.MinimumSpeedTemperature,];
    xData.AddRange(activePoints.Select(cp => (double)cp.Temperature));
    xData.Add((double)_fanSettings.PanicFromTemperature);

    List<double> yData = [(double)_fanSettings.MinimumSpeed,];
    yData.AddRange(activePoints.Select(cp => (double)cp.FanPercentage));
    yData.Add((double)_fanSettings.PanicSpeed);

    _curveGenerator = Fit.LogarithmFunc(xData.ToArray(), yData.ToArray());
  }

  private decimal ClampToPercentage(decimal val) => Math.Clamp(val, _fanSettings.MinimumSpeed, max: 100m);

  public decimal CalculateFanSpeed(decimal temperature)
  {
    decimal fromCurve = (decimal)_curveGenerator((double)temperature);
    return ClampToPercentage(fromCurve);
  }

  public void UpdateFanSettings(FanSettings fanSettings)
  {
    _fanSettings = fanSettings;
    ConstructCurveGenerator();
  }

  public void ResetFanSettings()
  {
    _fanSettings = _originalFanSettings with { };
    ConstructCurveGenerator();
  }
}