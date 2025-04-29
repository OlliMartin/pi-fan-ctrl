namespace PiFanCtrl.Services;

public class FanSpeedCalculator
{
  private int offThreshold = 30;
  private int panicFrom = 70;
  private decimal multiplier = 0.9m;

  public decimal CalculateFanSpeed(decimal temperature)
  {
    if (temperature < offThreshold)
    {
      return 0;
    }

    if (temperature >= panicFrom)
    {
      return 100;
    }

    return Math.Min(val1: 100, temperature * multiplier);
  }
}