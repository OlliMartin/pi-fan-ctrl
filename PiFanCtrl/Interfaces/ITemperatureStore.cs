using PiFanCtrl.Model;

namespace PiFanCtrl.Interfaces;

public interface ITemperatureStore
{
  public Task AddAsync(TemperatureReading reading, CancellationToken cancelToken = default);

  public Task AddRangeAsync(
    IEnumerable<TemperatureReading> readings,
    CancellationToken cancelToken = default
  );

  public IEnumerable<TemperatureReading> GetAll();
}