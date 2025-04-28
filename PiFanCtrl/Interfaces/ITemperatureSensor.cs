using PiFanCtrl.Model;

namespace PiFanCtrl.Interfaces;

public interface ITemperatureSensor
{
  string Name { get; }
  
  Task<IEnumerable<TemperatureReading>> ReadNextValuesAsync(CancellationToken cancelToken = default);
}