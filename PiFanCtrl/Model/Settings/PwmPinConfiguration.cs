namespace PiFanCtrl.Model.Settings;

public class PwmPinConfiguration
{
  public const string SECTION_NAME = "Pwm";
  
  public int ChipIndex { get; init; } = 0;
  public int Channel { get; init; } = 1;
}