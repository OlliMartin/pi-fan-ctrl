using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;

namespace PiFanCtrl.Services;

public class TemperatureWrapper(IEnumerable<ITemperatureSensor> sensors, ITemperatureStore temperatureStore)
  : ITemperatureSensor
{
  public string Name => "Aggregate";

  public async Task<TemperatureReading?> ReadNextValueAsync(CancellationToken cancelToken = default)
  {
    var values = (await Task.WhenAll(sensors.Select(s => s.ReadNextValueAsync(cancelToken))))
      .OfType<TemperatureReading>()
      .ToList();

    if (values.Count == 0)
    {
      return null;
    }

    var overrides = values
      .Where(v => v.IsOverride)
      .ToList();

    var result = overrides.Count > 0
      ? new TemperatureReading()
      {
        Sensor = Name,
        IsOverride = true,
        Value = ProcessReadings(overrides)
      }
      : new TemperatureReading()
      {
        Sensor = Name,
        IsOverride = false,
        Value = ProcessReadings(values)
      };
    
    temperatureStore.Add(result);

    return result;
  }

  private decimal ProcessReadings(IList<TemperatureReading> readings)
  {
    temperatureStore.AddRange(readings);

    // TODO: Make configurable
    var result = readings.Average(o => o.Value);
    
    return result;
  }
}