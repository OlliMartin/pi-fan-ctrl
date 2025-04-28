using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;

namespace PiFanCtrl.Services.FanRpm;

public class DummyFanRpmSensor : IFanRpmSensor
{
  public string Name => "DummyRpmSensor";

  public Task<FanRpmReading?> ReadNextValueAsync(CancellationToken cancelToken = default) =>
    Task.FromResult<FanRpmReading?>(
      new()
      {
        Source = Name,
        Value = 0,
      }
    );
}