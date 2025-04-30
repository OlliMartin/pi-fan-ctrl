namespace PiFanCtrl.Model.Settings;

public class I2CSensorConfiguration
{
  public int BusId { get; init; }

  public int? I2CAddress { get; init; }
}