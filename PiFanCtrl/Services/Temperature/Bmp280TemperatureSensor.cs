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
  private const int DEVICE_OPEN_RETRIES = 5;

  private readonly ILogger<Bmp280TemperatureSensor> _logger;
  private readonly I2CSensorConfiguration _sensorConfiguration;
  public string Name => $"BMP280-Bus-{_sensorConfiguration.I2CAddress}-Addr-{_i2cAddress}";

  private readonly int _i2cAddress;
  private I2cDevice? _i2cDevice;
  private Bmp280? _sensor;

  public Bmp280TemperatureSensor(
    ILogger<Bmp280TemperatureSensor> logger,
    I2CSensorConfiguration sensorConfiguration
  )
  {
    _logger = logger;
    _sensorConfiguration = sensorConfiguration;

    _i2cAddress = _sensorConfiguration.I2CAddress ?? Bmp280.DefaultI2cAddress;

    try
    {
      CreateSensor(sensorConfiguration);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Cannot open temperature sensor.");
    }
  }

  private void CreateSensor(I2CSensorConfiguration sensorConfiguration)
  {
    Stopwatch sw = Stopwatch.StartNew();
    int i = 1;

    for (; i <= DEVICE_OPEN_RETRIES; i++)
      try
      {
        _logger.LogDebug(
          "Starting temperature sensor on bus {busId} and address {addr}.",
          sensorConfiguration.BusId,
          _i2cAddress
        );

        I2cConnectionSettings i2cSettings = new(_sensorConfiguration.BusId, _i2cAddress);
        _i2cDevice = I2cDevice.Create(i2cSettings);
        _sensor = new(_i2cDevice);

        break;
      }
      catch (Exception ex)
      {
        _logger.LogWarning(
          ex,
          "An error occurred opening device {dev}. Retrying up to {cnt} times.",
          Name,
          DEVICE_OPEN_RETRIES
        );

        if (i == DEVICE_OPEN_RETRIES)
        {
          throw;
        }
      }

    sw.Stop();

    _logger.LogInformation(
      "Temperature sensor {name} connected after {retryCount} attempts in {elapsed}.",
      Name,
      i,
      sw.Elapsed
    );

    if (_sensor is not null)
    {
      _sensor.TemperatureSampling = Sampling.LowPower;
      _sensor.PressureSampling = Sampling.UltraHighResolution;
    }
  }

  public async Task<IEnumerable<TemperatureReading>> ReadNextValuesAsync(
    CancellationToken cancelToken = default
  )
  {
    Stopwatch sw = Stopwatch.StartNew();
    decimal? val = null;

    try
    {
      Bmp280ReadResult? reading =
        await (_sensor?.ReadAsync() ?? Task.FromResult<Bmp280ReadResult>(null!));

      val = (decimal?)reading?.Temperature?.Value;

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