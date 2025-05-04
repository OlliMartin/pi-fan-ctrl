using Microsoft.AspNetCore.Mvc;
using PiFanControl.Abstractions;
using PiFanCtrl.Services;

namespace PiFanCtrl.Controllers;

public class SystemInfoController : ControllerBase
{
  private readonly SystemInfoProvider _systemInfoProvider;
  
  public SystemInfoController(SystemInfoProvider systemInfoProvider)
  {
    _systemInfoProvider = systemInfoProvider;
  }
  
  [HttpGet("/info")]
  public async Task<IActionResult> GetAsync()
  {
    SystemInfo systemInfo = await _systemInfoProvider.GetLatestSysInfoAsync(HttpContext.RequestAborted);
    return Ok(systemInfo);
  }
}