using System.Text.Json.Serialization;

namespace PiFanCtrl.Model.Unifi;

public class TokenRequest(string username, string password)
{
  [JsonPropertyName("username")]
  public string Username { get; } = username;

  [JsonPropertyName("password")]
  public string Password { get; } = password;

  [JsonPropertyName("rememberMe")]
  public bool RememberMe => false;

  [JsonPropertyName("token")]
  public string Token => string.Empty;
}

public class TokenResponse
{
  [JsonPropertyName("deviceToken")]
  public string DeviceToken { get; init; }
}