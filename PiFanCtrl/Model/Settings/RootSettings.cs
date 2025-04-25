namespace PiFanCtrl.Model.Settings;

public class RootSettings
{
  public bool AllowTemperatureSimulation { get; init; } = false;
  
  public PwmPinConfiguration PwmPin { get; init; } = new();

  public TemperatureSensorConfiguration Temperature { get; init; } = new();
}