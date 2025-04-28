using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;
using PiFanCtrl.Model.Settings;
using PiFanCtrl.Model.Unifi;

namespace PiFanCtrl.Services.Temperature;

public class UnifiTemperatureSensor : ITemperatureSensor
{
  private const string DEVICE_ENDPOINT = "proxy/network/api/s/default/stat/device";

  private readonly ILogger<UnifiTemperatureSensor> _logger;
  private readonly IOptions<UnifiSensorsConfiguration> _unifiOptions;
  private readonly IHttpClientFactory _httpClientFactory;

  public UnifiTemperatureSensor(
    ILogger<UnifiTemperatureSensor> logger,
    IOptions<UnifiSensorsConfiguration> unifiOptions,
    IHttpClientFactory httpClientFactory
  )
  {
    _logger = logger;
    _unifiOptions = unifiOptions;
    _httpClientFactory = httpClientFactory;
  }

  public string Name => "Unifi";

  public async Task<IEnumerable<TemperatureReading>> ReadNextValuesAsync(
    CancellationToken cancelToken = default
  )
  {
    using HttpClient httpClient = _httpClientFactory.CreateClient(nameof(UnifiTemperatureSensor));

    try
    {
      DeviceResponse? response = await GetDeviceInfoAsync(cancelToken, httpClient);

      if (response is null)
      {
        return [];
      }

      UnifiSensorsConfiguration settings = _unifiOptions.Value;

      IEnumerable<((string Name, string Suffix, decimal Value) Triple, UnifiDevice Device)> mappedToSensor =
        response
          .Data
          .Where(device => device.CanGetTemperature())
          .Where(device => settings.Devices.SingleOrDefault(sd => sd.Name == device.Name) is not null)
          .SelectMany(device => device.GetReadings())
          .Select(device => (device, settings.Devices.Single(sd => sd.Name == device.Name)));

      return mappedToSensor.Select(
        (tuple) => new TemperatureReading()
        {
          Sensor = $"unifi-{tuple.Device.FriendlyName}{tuple.Triple.Suffix}",
          // TODO: This function is wrong; Figure out how to retrieve data.
          Value = tuple.Triple.Value,
        }
      );
    }
    catch (OperationCanceledException)
    {
      return [];
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "An error occurred querying unifi devices. Is the password wrong?");
      return [];
    }
  }

  private async Task<DeviceResponse?> GetDeviceInfoAsync(CancellationToken cancelToken, HttpClient httpClient)
  {
    Stopwatch sw = Stopwatch.StartNew();
    long contentSize = 0;

    try
    {
      using HttpResponseMessage httpResponse = await httpClient.GetAsync(DEVICE_ENDPOINT, cancelToken);
      httpResponse.EnsureSuccessStatusCode();

      contentSize = httpResponse.Content.Headers.ContentLength ?? 0;
      DeviceResponse? response = await httpResponse.Content.ReadFromJsonAsync<DeviceResponse>(cancelToken);

      if (response is null)
      {
        _logger.LogWarning("Could not parse unifi device response.");
      }

      return response;
    }
    finally
    {
      sw.Stop();

      _logger.LogDebug(
        "Retrieved device information from unifi controller in {elapsed} (Content-Size={cs}).",
        sw.Elapsed,
        contentSize
      );
    }
  }
}

[UsedImplicitly]
public class UnifiAuthenticationMessageHandler : DelegatingHandler
{
  private readonly ILogger _logger;
  private readonly IOptions<UnifiSensorsConfiguration> _configuration;
  private readonly JwtSecurityTokenHandler _tokenParser = new();
  private readonly Uri _tokenPath = new("/api/auth/login", UriKind.Relative);

  private string? _accessToken;
  private DateTime _expiresAt;

  public UnifiAuthenticationMessageHandler(
    ILogger<UnifiAuthenticationMessageHandler> logger,
    IOptions<UnifiSensorsConfiguration> configuration
  )
  {
    _logger = logger;
    _configuration = configuration;
  }

  protected async override Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request,
    CancellationToken cancellationToken
  )
  {
    string accessToken = await GetAccessTokenAsync(cancellationToken);
    request.Headers.Authorization = new("Bearer", accessToken);
    return await base.SendAsync(request, cancellationToken);
  }

  private async Task<string> GetAccessTokenAsync(CancellationToken cancelToken)
  {
    UnifiSensorsConfiguration settings = _configuration.Value;

    if (_accessToken is not null && _expiresAt > DateTime.UtcNow)
    {
      _logger.LogTrace(
        "Token present in cache and not expired. Using it. (Exp={exp}, CS={cs})",
        _expiresAt,
        settings.ClockSkew
      );

      return _accessToken;
    }

    Stopwatch sw = Stopwatch.StartNew();

    using HttpRequestMessage message = new();

    message.RequestUri = new(settings.BaseUri, _tokenPath);
    message.Method = HttpMethod.Post;
    message.Content = JsonContent.Create(new TokenRequest(settings.Username, settings.Password));

    HttpResponseMessage response = await base.SendAsync(
      message,
      cancelToken
    );

    response.EnsureSuccessStatusCode();
    TokenResponse? parsed = await response.Content.ReadFromJsonAsync<TokenResponse>(cancelToken);

    if (parsed is null)
    {
      throw new InvalidOperationException("Could not obtain unifi token.");
    }

    string result = ProcessJwt(settings, parsed.DeviceToken);

    sw.Stop();

    _logger.LogInformation(
      "Retrieved auth token in {elapsed}. Token will be used until {eat} (clock skew={cs}).",
      sw.Elapsed,
      _expiresAt,
      settings.ClockSkew
    );

    return result;
  }

  private string ProcessJwt(UnifiSensorsConfiguration settings, string encodedToken)
  {
    JwtSecurityToken? decoded = _tokenParser.ReadJwtToken(encodedToken);

    if (decoded is null)
    {
      throw new InvalidOperationException("Could not parse unifi JWT.");
    }

    _accessToken = encodedToken;

    _expiresAt = (decoded.IssuedAt != DateTime.MinValue
      ? decoded.IssuedAt
      : DateTime.UtcNow) + settings.TokenExpiresAfter - settings.ClockSkew;

    return _accessToken;
  }
}