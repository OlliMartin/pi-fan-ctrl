using PiFanCtrl.Model;

namespace PiFanCtrl.Interfaces;

public interface ITemperatureStore
{
  public void Add(TemperatureReading reading);

  public void AddRange(IEnumerable<TemperatureReading> readings);

  public IEnumerable<TemperatureReading> GetAll();
}