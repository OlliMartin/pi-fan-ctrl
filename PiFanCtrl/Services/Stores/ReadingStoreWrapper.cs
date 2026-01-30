using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;

namespace PiFanCtrl.Services.Stores;

public class ReadingStoreWrapper : IReadingStore
{
  private readonly IEnumerable<IReadingStore> _stores;
  private readonly HashSet<string> _knownSources = new();
  private readonly object _sourcesLock = new();

  public event EventHandler<ReadingChangedEventArgs>? ReadingChanged;

  public ReadingStoreWrapper(IEnumerable<IReadingStore> stores)
  {
    _stores = stores;

    // Subscribe to ReadingChanged events from all underlying stores
    foreach (var store in _stores)
    {
      store.ReadingChanged += OnUnderlyingStoreReadingChanged;
    }
  }

  private void OnUnderlyingStoreReadingChanged(object? sender, ReadingChangedEventArgs e)
  {
    lock (_sourcesLock)
    {
      _knownSources.Add(e.Source);
    }

    // Forward the event to our subscribers
    ReadingChanged?.Invoke(this, e);
  }

  public async Task AddAsync(IReading reading, CancellationToken cancelToken = default)
    => await Task.WhenAll(_stores.Select(store => store.AddAsync(reading, cancelToken)));

  public async Task AddRangeAsync(
    IEnumerable<IReading> readings,
    CancellationToken cancelToken = default
  )
    => await Task.WhenAll(_stores.Select(store => store.AddRangeAsync(readings, cancelToken)));

  public IEnumerable<IReading> GetAll() => throw new NotImplementedException();

  public decimal? GetLatest(string source)
  {
    // Get the latest value from the first store that has it (typically SlidingReadingStore)
    foreach (var store in _stores)
    {
      var value = store.GetLatest(source);
      if (value.HasValue)
      {
        return value;
      }
    }
    return null;
  }

  public IEnumerable<string> GetKnownSources()
  {
    lock (_sourcesLock)
    {
      return _knownSources.ToList();
    }
  }
}