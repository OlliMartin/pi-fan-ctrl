using PiFanCtrl.Model;

namespace PiFanCtrl.Interfaces;

public interface IFanRpmSensor
{
  string Name { get; }

  Task<FanRpmReading?> ReadNextValueAsync(CancellationToken cancelToken = default);
}