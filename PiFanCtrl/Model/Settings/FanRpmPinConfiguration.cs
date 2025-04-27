namespace PiFanCtrl.Model.Settings;

public class FanRpmPinConfiguration
{
  public const string SECTION_NAME = "Rpm";
  
  public int Pin { get; init; } = 24;
}