using System.Device.I2c;
using System.Diagnostics;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.ReadResult;
using Iot.Device.DHTxx;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;
using PiFanCtrl.Model.Settings;

namespace PiFanCtrl.Services.Temperature;

public sealed class Bmp280TemperatureSensor : ITemperatureSensor, IDisposable
{
  private readonly ILogger<Bmp280TemperatureSensor> _logger;
  private readonly I2CSensorConfiguration _sensorConfiguration;
  public string Name => $"BMP280-Bus-{_sensorConfiguration.I2CAddress}-Addr-{_i2cAddress}";

  private readonly int _i2cAddress;
  private readonly I2cDevice _i2cDevice;
  private readonly Bmp280 _sensor;

  public Bmp280TemperatureSensor(
    ILogger<Bmp280TemperatureSensor> logger,
    I2CSensorConfiguration sensorConfiguration
  )
  {
    _logger = logger;
    _sensorConfiguration = sensorConfiguration;

    _i2cAddress = _sensorConfiguration.I2CAddress ?? Bmp280.DefaultI2cAddress;

    _logger.LogInformation("Hello I2C!");
    using I2cDevice i2c = I2cDevice.Create(new(busId: 1, deviceAddress: 0x12));
    i2c.WriteByte(value: 0x42);
    byte read = i2c.ReadByte();

    _logger.LogInformation("I2C result {res}", read);

    _logger.LogInformation(
      "Starting temperature sensor on bus {busId} and address {addr}.",
      sensorConfiguration.BusId,
      _i2cAddress
    );

    I2cConnectionSettings i2cSettings = new(_sensorConfiguration.BusId, _i2cAddress);
    _i2cDevice = I2cDevice.Create(i2cSettings);
    _sensor = new(_i2cDevice);

    _sensor.TemperatureSampling = Sampling.LowPower;
    _sensor.PressureSampling = Sampling.UltraHighResolution;
  }

  public async Task<IEnumerable<TemperatureReading>> ReadNextValuesAsync(
    CancellationToken cancelToken = default
  )
  {
    Stopwatch sw = Stopwatch.StartNew();
    decimal? val = null;

    try
    {
      Bmp280ReadResult reading = await _sensor.ReadAsync();
      val = (decimal?)reading.Temperature?.Value;

      if (val is not null)
      {
        return
        [
          new()
          {
            Source = Name,
            Value = val.Value,
          },
        ];
      }

      return [];
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An error occurred retrieving temperature value.");
      return [];
    }
    finally
    {
      sw.Stop();
      _logger.LogDebug("{sName} read temperature {val} in {elapsed}.", Name, val ?? int.MinValue, sw.Elapsed);
    }
  }

  public void Dispose()
  {
    Stopwatch sw = Stopwatch.StartNew();
    _logger.LogInformation("Disposing {name}.", nameof(Bmp280TemperatureSensor));

    _sensor.Dispose();
    _i2cDevice.Dispose();

    sw.Stop();
    _logger.LogDebug("{name} disposed in {elapsed}.", nameof(Bmp280TemperatureSensor), sw.Elapsed);
  }
}