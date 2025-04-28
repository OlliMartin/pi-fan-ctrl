using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;

namespace PiFanCtrl.Services;

public class TemperatureWrapper(IEnumerable<ITemperatureSensor> sensors, ITemperatureStore temperatureStore)
  : ITemperatureSensor
{
  public string Name => "Aggregate";

  public async Task<IEnumerable<TemperatureReading>> ReadNextValuesAsync(
    CancellationToken cancelToken = default
  )
  {
    List<TemperatureReading> values =
      (await Task.WhenAll(sensors.Select(s => s.ReadNextValuesAsync(cancelToken))))
      .SelectMany(e => e)
      .ToList();

    if (values.Count == 0)
    {
      return [];
    }

    List<TemperatureReading> overrides = values
      .Where(v => v.IsOverride)
      .ToList();

    TemperatureReading result = overrides.Count > 0
      ? new()
      {
        Sensor = Name,
        IsOverride = true,
        Value = ProcessReadings(overrides),
      }
      : new TemperatureReading()
      {
        Sensor = Name,
        IsOverride = false,
        Value = ProcessReadings(values),
      };

    temperatureStore.Add(result);

    return [result,];
  }

  private decimal ProcessReadings(IList<TemperatureReading> readings)
  {
    temperatureStore.AddRange(readings);

    // TODO: Make configurable
    decimal result = readings.Average(o => o.Value);

    return result;
  }
}