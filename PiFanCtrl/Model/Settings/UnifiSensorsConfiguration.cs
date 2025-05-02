namespace PiFanCtrl.Model.Settings;

public class UnifiDevice
{
  public string Name { get; init; }

  public string FriendlyName { get; init; }

  public bool Active { get; init; } = true;
}

public class UnifiSensorsConfiguration : SensorConfiguration
{
  public string BaseUrl { get; init; }

  public string Username { get; init; }

  public bool Secure { get; init; }

  public string Password { get; init; }

  public IList<UnifiDevice> Devices { get; init; }

  public TimeSpan ClockSkew { get; init; } = TimeSpan.FromSeconds(seconds: 30);

  public TimeSpan TokenExpiresAfter { get; init; } = TimeSpan.FromMinutes(minutes: 15);

  private Uri? _baseUri;

  public Uri BaseUri
  {
    get
    {
      if (_baseUri is not null)
      {
        return _baseUri;
      }

      string prefix = Secure
        ? "https://"
        : "http://";

      _baseUri = new(
        $"{prefix}{BaseUrl}"
      );

      return _baseUri;
    }
  }
}