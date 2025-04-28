using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;

namespace PiFanCtrl.Services.Temperature;

public class DummyTemperatureSensor : ITemperatureSensor
{
  private decimal? _value;

  public string Name => "Simulated";

  public void Simulate(decimal value)
  {
    _value = value;
  }

  public void Reset()
  {
    _value = null;
  }

  public Task<IEnumerable<TemperatureReading>> ReadNextValuesAsync(CancellationToken cancelToken = default)
  {
    if (_value is null)
    {
      return Task.FromResult<IEnumerable<TemperatureReading>>(Array.Empty<TemperatureReading>());
    }

    return Task.FromResult<IEnumerable<TemperatureReading>>(
      [
        new() { Value = _value.Value, IsOverride = true, Sensor = Name, },
      ]
    );
  }
}