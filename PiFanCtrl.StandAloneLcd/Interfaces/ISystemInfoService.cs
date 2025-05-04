using PiFanControl.Abstractions;

namespace PiFanCtrl.StandAloneLcd.Interfaces;

public interface ISystemInfoService
{
  public Task<SystemInfo> GetSystemInfoAsync(CancellationToken cancellationToken);
}