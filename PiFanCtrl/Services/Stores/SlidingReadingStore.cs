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

  public Task AddAsync(IReading reading, CancellationToken cancelToken = default)
  {
    _buffer.PushFront(reading);
    return Task.CompletedTask;
  }

  public Task AddRangeAsync(IEnumerable<IReading> readings, CancellationToken cancelToken = default)
  {
    foreach (IReading reading in readings) _buffer.PushFront(reading);

    return Task.CompletedTask;
  }

  public IEnumerable<IReading> GetAll() => _buffer;
}