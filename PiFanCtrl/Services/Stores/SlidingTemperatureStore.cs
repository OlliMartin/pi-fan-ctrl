using PiFanCtrl.DataStructures;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;

namespace PiFanCtrl.Services.Stores;

public class SlidingTemperatureStore : ITemperatureStore
{
  private const int WINDOW_SIZE = 10;
  private readonly CircularBuffer<TemperatureReading> _buffer = new(WINDOW_SIZE);

  public void Add(TemperatureReading reading)
  {
    _buffer.PushFront(reading);
  }

  public void AddRange(IEnumerable<TemperatureReading> readings)
  {
    foreach (var reading in readings)
    {
      _buffer.PushFront(reading);
    }
  }

  public IEnumerable<TemperatureReading> GetAll()
  {
    return _buffer;
  }
}