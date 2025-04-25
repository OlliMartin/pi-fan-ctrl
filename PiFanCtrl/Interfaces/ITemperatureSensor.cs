using PiFanCtrl.Model;

namespace PiFanCtrl.Interfaces;

public interface ITemperatureSensor
{
  string Name { get; }
  
  Task<TemperatureReading?> ReadNextValueAsync(CancellationToken cancelToken = default);
}