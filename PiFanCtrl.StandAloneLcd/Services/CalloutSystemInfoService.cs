using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using PiFanControl.Abstractions;
using PiFanCtrl.StandAloneLcd.Interfaces;

namespace PiFanCtrl.StandAloneLcd.Services;

public class CalloutSystemInfoService(ILogger<CalloutSystemInfoService> logger, IHttpClientFactory httpClientFactory) : ISystemInfoService
{
  public async Task<SystemInfo> GetSystemInfoAsync(CancellationToken cancellationToken)
  {
    try
    {
      using HttpClient httpClient = httpClientFactory.CreateClient(nameof(CalloutSystemInfoService));
      using HttpResponseMessage httpResponseMessage =  await httpClient.GetAsync("/info", cancellationToken);

      if (httpResponseMessage.IsSuccessStatusCode)
      {
        return await httpResponseMessage.Content.ReadFromJsonAsync<SystemInfo>(cancellationToken) ?? new SystemInfo();
      }
      
      logger.LogWarning("Unexpected status code {sc}. Cannot process readings.", httpResponseMessage.StatusCode); 
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An error occurred querying actuals.");
    }

    return new SystemInfo();
  }
}