using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;

namespace PiFanCtrl.Services.FanRpm;

public class DummyFanRpmSensor : IFanRpmSensor
{
  public string Name => "Simulated";
  
  public Task<FanRpmReading?> ReadNextValueAsync(CancellationToken cancelToken = default)
  {
    return Task.FromResult<FanRpmReading?>(new FanRpmReading()
    {
      Sensor = Name,
      Value = 0
    });
  }
}