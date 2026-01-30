using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;

namespace PiFanCtrl.Services.Stores;

public class ReadingStoreWrapper(IEnumerable<IReadingStore> stores) : IReadingStore
{
  public event EventHandler<ReadingChangedEventArgs>? ReadingChanged;

  public async Task AddAsync(IReading reading, CancellationToken cancelToken = default)
    => await Task.WhenAll(stores.Select(store => store.AddAsync(reading, cancelToken)));

  public async Task AddRangeAsync(
    IEnumerable<IReading> readings,
    CancellationToken cancelToken = default
  )
    => await Task.WhenAll(stores.Select(store => store.AddRangeAsync(readings, cancelToken)));

  public IEnumerable<IReading> GetAll() => throw new NotImplementedException();
}