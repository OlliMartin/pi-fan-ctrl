using Iot.Device.DHTxx;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;
using PiFanCtrl.Model.Settings;
using UnitsNet;

namespace PiFanCtrl.Services.Temperature;

public sealed class DHT22TemperatureSensor : ITemperatureSensor, IDisposable
{
  private readonly ILogger<DHT22TemperatureSensor> _logger;
  private readonly SensorConfiguration _sensorConfiguration;
  public string Name => $"DHT22-Pin-{_sensorConfiguration.Pin}";

  private readonly DhtBase _dht;
  
  public DHT22TemperatureSensor(ILogger<DHT22TemperatureSensor> logger, SensorConfiguration sensorConfiguration)
  {
    _logger = logger;
    _sensorConfiguration = sensorConfiguration;
    
    _logger.LogInformation("Starting temperature sensor on pin {pin}.", sensorConfiguration.Pin);
    
    _dht = new Dht22(_sensorConfiguration.Pin);
  }
  
  public Task<TemperatureReading?> ReadNextValueAsync(CancellationToken cancelToken = default)
  {
    var success = _dht.TryReadTemperature(out var temperature);

    if (success)
    {
      return Task.FromResult<TemperatureReading?>(new()
      {
        Sensor = Name,
        IsOverride = false,
        Value = (decimal)temperature.Value
      });
    }

    return Task.FromResult<TemperatureReading?>(null);
  }

  public void Dispose()
  {
    _dht.Dispose();
  }
}