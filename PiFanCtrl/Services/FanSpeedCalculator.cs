using MathNet.Numerics;

namespace PiFanCtrl.Services;

public class FanSpeedCalculator
{
  private int minSpeed = 30;

  private int offThreshold = 20;
  private int panicFrom = 55;
  private decimal multiplier = 0.9m;

  private Func<double, double> _logarithmic;

  public FanSpeedCalculator()
  {
    double[] xdata = new double[] { offThreshold, panicFrom, };
    double[] ydata = new double[] { 0, 100, };

    _logarithmic = Fit.LogarithmFunc(xdata, ydata);
  }

  private decimal ClampToPercentage(decimal val) => Math.Clamp(val, minSpeed, max: 100);

  public decimal CalculateFanSpeed(decimal temperature) =>
    // if (temperature < offThreshold)
    // {
    //   return 0;
    // }
    //
    // if (temperature >= panicFrom)
    // {
    //   return 100;
    // }
    //
    // return Math.Min(val1: 100, temperature * multiplier);
    ClampToPercentage((decimal)_logarithmic((double)temperature));
}