namespace PiFanCtrl.Model.Settings;

public class RootSettings
{
  public const string SECTION_NAME = "FanControl";
  
  public bool AllowTemperatureSimulation { get; init; } = false;
  
  public PwmPinConfiguration PwmPin { get; init; } = new();

  public TemperatureSensorConfiguration Temperature { get; init; } = new();

  public FanRpmPinConfiguration FanRpmPin { get; init; } = new();
}