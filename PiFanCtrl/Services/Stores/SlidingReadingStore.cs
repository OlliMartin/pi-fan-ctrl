using Microsoft.Extensions.Options;
using PiFanCtrl.DataStructures;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;
using PiFanCtrl.Model.Settings;

namespace PiFanCtrl.Services.Stores;

public class SlidingReadingStore : IReadingStore
{
  private const int WINDOW_SIZE = 1000;
  private readonly CircularBuffer<IReading> _buffer = new(WINDOW_SIZE);
  private readonly object _lock = new();

  public event EventHandler<ReadingChangedEventArgs>? ReadingChanged;

  public Task AddAsync(IReading reading, CancellationToken cancelToken = default)
  {
    lock (_lock)
    {
      _buffer.PushFront(reading);
      ReadingChanged?.Invoke(this, new ReadingChangedEventArgs(reading.Source, reading));
    }
    return Task.CompletedTask;
  }

  public Task AddRangeAsync(IEnumerable<IReading> readings, CancellationToken cancelToken = default)
  {
    lock (_lock)
    {
      foreach (IReading reading in readings)
      {
        _buffer.PushFront(reading);
        ReadingChanged?.Invoke(this, new ReadingChangedEventArgs(reading.Source, reading));
      }
    }

    return Task.CompletedTask;
  }

  public IEnumerable<IReading> GetAll()
  {
    lock (_lock)
    {
      return _buffer.ToList(); // Return a copy to avoid concurrent modification
    }
  }

  public decimal? GetLatest(string source)
  {
    lock (_lock)
    {
      return _buffer.Where(r => r.Source == source).OrderByDescending(r => r.AsOf).FirstOrDefault()?.Value;
    }
  }
}