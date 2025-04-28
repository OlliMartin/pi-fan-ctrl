namespace PiFanCtrl.Model.Settings;

public class InfluxConfiguration
{
  public const string SECTION_NAME = "Influx";

  public string Url { get; init; }

  public string Password { get; init; }

  public string Organisation { get; init; }

  public string Bucket { get; init; }
}