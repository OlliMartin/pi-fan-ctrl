using System.Device.I2c;
using System.Diagnostics;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.ReadResult;
using Iot.Device.DHTxx;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;
using PiFanCtrl.Model.Settings;

namespace PiFanCtrl.Services.Temperature;

public sealed class Bme280TemperatureSensor : BmX280TemperatureSensor
{
  public Bme280TemperatureSensor(
    ILogger<Bme280TemperatureSensor> logger,
    I2CSensorConfiguration sensorConfiguration
  ) : base(logger, sensorConfiguration)
  {
  }

  protected override string SensorTypeName => "BMP280";

  protected override Func<I2cDevice, Bmx280Base> SensorFactory { get; }
    = (device) => new Bme280(device);
}