using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;

namespace PiFanCtrl.Services.Stores;

public class TemperatureStoreWrapper(IEnumerable<ITemperatureStore> stores) : ITemperatureStore
{
  public async Task AddAsync(TemperatureReading reading, CancellationToken cancelToken = default)
    => await Task.WhenAll(stores.Select(store => store.AddAsync(reading, cancelToken)));

  public async Task AddRangeAsync(
    IEnumerable<TemperatureReading> readings,
    CancellationToken cancelToken = default
  )
    => await Task.WhenAll(stores.Select(store => store.AddRangeAsync(readings, cancelToken)));

  public IEnumerable<TemperatureReading> GetAll() => throw new NotImplementedException();
}