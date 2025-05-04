using PiFanControl.Abstractions;

namespace PiFanCtrl.StandAloneLcd.Interfaces;

public interface IDisplay
{
  Task Draw(SystemInfo systemInfo, CancellationToken cancelToken = default);
}