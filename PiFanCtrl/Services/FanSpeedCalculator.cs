using System.Diagnostics.CodeAnalysis;
using MathNet.Numerics;
using MathNet.Numerics.LinearRegression;
using Microsoft.Extensions.Options;
using PiFanCtrl.Model;

namespace PiFanCtrl.Services;

public class FanSpeedCalculator
{
  private readonly ILogger<FanSpeedCalculator> _logger;
  private Func<double, double> _curveGenerator;

  private FanSettings _fanSettings;
  private readonly FanSettings _originalFanSettings;

  public FanSettings FanSettings => _fanSettings with { };

  public FanSpeedCalculator(ILogger<FanSpeedCalculator> logger, IOptions<FanSettings> fanSettingsOptions)
  {
    _logger = logger;
    _fanSettings = fanSettingsOptions.Value;

    // Deep copy the original settings including CurvePoints
    _originalFanSettings = _fanSettings with
    {
      CurvePoints = _fanSettings.CurvePoints.Select(cp => cp with { }).ToList(),
    };

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
    try
    {
      decimal fromCurve = (decimal)_curveGenerator((double)temperature);
      return ClampToPercentage(fromCurve);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error calculating fan speed from curve generator.");
      return 100m;
    }
  }

  public void UpdateFanSettings(FanSettings fanSettings)
  {
    _fanSettings = fanSettings;
    ConstructCurveGenerator();
  }

  public void ResetFanSettings()
  {
    // Deep copy the original settings including CurvePoints
    _fanSettings = _originalFanSettings with
    {
      CurvePoints = _originalFanSettings.CurvePoints.Select(cp => cp with { }).ToList(),
    };

    ConstructCurveGenerator();
  }
}