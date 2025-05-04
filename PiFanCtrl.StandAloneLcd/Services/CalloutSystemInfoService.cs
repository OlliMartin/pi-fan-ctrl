using System.Net.Http.Json;
using PiFanControl.Abstractions;
using PiFanCtrl.StandAloneLcd.Interfaces;

namespace PiFanCtrl.StandAloneLcd.Services;

public class CalloutSystemInfoService(IHttpClientFactory httpClientFactory) : ISystemInfoService
{
  public async Task<SystemInfo> GetSystemInfoAsync(CancellationToken cancellationToken)
  {
    using HttpClient httpClient = httpClientFactory.CreateClient(nameof(CalloutSystemInfoService));
    using HttpResponseMessage httpResponseMessage =  await httpClient.GetAsync("/info", cancellationToken);

    if (httpResponseMessage.IsSuccessStatusCode)
    {
      return await httpResponseMessage.Content.ReadFromJsonAsync<SystemInfo>(cancellationToken) ?? new SystemInfo();
    }

    return new SystemInfo();
  }
}