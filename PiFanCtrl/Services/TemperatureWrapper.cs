using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;

namespace PiFanCtrl.Services;

public class TemperatureWrapper(
  IEnumerable<ITemperatureSensor> sensors,
  [FromKeyedServices("delegating")] ITemperatureStore temperatureStore
)
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
        Value = await ProcessReadingsAsync(overrides, cancelToken),
      }
      : new TemperatureReading()
      {
        Sensor = Name,
        IsOverride = false,
        Value = await ProcessReadingsAsync(values, cancelToken),
      };

    await temperatureStore.AddAsync(result, cancelToken);

    return [result,];
  }

  private async Task<decimal> ProcessReadingsAsync(
    IList<TemperatureReading> readings,
    CancellationToken cancelToken
  )
  {
    await temperatureStore.AddRangeAsync(readings, cancelToken);

    // TODO: Make configurable
    decimal result = readings.Average(o => o.Value);

    return result;
  }
}