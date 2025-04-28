using Microsoft.Extensions.Options;
using PiFanCtrl.DataStructures;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;
using PiFanCtrl.Model.Settings;

namespace PiFanCtrl.Services.Stores;

public class SlidingTemperatureStore : ITemperatureStore
{
  private const int WINDOW_SIZE = 1000;
  private readonly CircularBuffer<TemperatureReading> _buffer = new(WINDOW_SIZE);

  public Task AddAsync(TemperatureReading reading, CancellationToken cancelToken = default)
  {
    _buffer.PushFront(reading);
    return Task.CompletedTask;
  }

  public Task AddRangeAsync(IEnumerable<TemperatureReading> readings, CancellationToken cancelToken = default)
  {
    foreach (TemperatureReading reading in readings) _buffer.PushFront(reading);

    return Task.CompletedTask;
  }

  public IEnumerable<TemperatureReading> GetAll() => _buffer;
}