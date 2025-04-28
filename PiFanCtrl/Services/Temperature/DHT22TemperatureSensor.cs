using System.Diagnostics;
using Iot.Device.DHTxx;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;
using PiFanCtrl.Model.Settings;
using UnitsNet;

namespace PiFanCtrl.Services.Temperature;

public sealed class DHT22TemperatureSensor : ITemperatureSensor, IDisposable
{
  private readonly ILogger<DHT22TemperatureSensor> _logger;
  private readonly HardwareSensorConfiguration _sensorConfiguration;
  public string Name => $"DHT22-Pin-{_sensorConfiguration.Pin}";

  private readonly Dht22 _dht;

  public DHT22TemperatureSensor(
    ILogger<DHT22TemperatureSensor> logger,
    HardwareSensorConfiguration sensorConfiguration
  )
  {
    _logger = logger;
    _sensorConfiguration = sensorConfiguration;

    _logger.LogInformation("Starting temperature sensor on pin {pin}.", sensorConfiguration.Pin);

    _dht = new(_sensorConfiguration.Pin);
  }

  public Task<IEnumerable<TemperatureReading>> ReadNextValuesAsync(CancellationToken cancelToken = default)
  {
    bool success = _dht.TryReadTemperature(out UnitsNet.Temperature temperature);

    if (success)
    {
      return Task.FromResult<IEnumerable<TemperatureReading>>(
        [
          new()
          {
            Source = Name,
            IsOverride = false,
            Value = (decimal)temperature.Value,
          },
        ]
      );
    }

    _logger.LogWarning("Failed to read temperature sensor {name}.", Name);

    return Task.FromResult<IEnumerable<TemperatureReading>>(Array.Empty<TemperatureReading>());
  }

  public void Dispose()
  {
    Stopwatch sw = Stopwatch.StartNew();
    _logger.LogInformation("Disposing {name}.", nameof(DHT22TemperatureSensor));

    _dht.Dispose();

    sw.Stop();
    _logger.LogDebug("{name} disposed in {elapsed}.", nameof(DHT22TemperatureSensor), sw.Elapsed);
  }
}