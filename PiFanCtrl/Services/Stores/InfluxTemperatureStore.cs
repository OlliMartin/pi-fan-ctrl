using System.Globalization;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Options;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;
using PiFanCtrl.Model.Settings;

namespace PiFanCtrl.Services.Stores;

public sealed class InfluxTemperatureStore(IOptions<InfluxConfiguration> influxConfiguration)
  : ITemperatureStore, IDisposable
{
  private readonly InfluxDBClient _influx = new(
    influxConfiguration.Value.Url,
    influxConfiguration.Value.Password
  );

  private readonly string _hostname = System.Net.Dns.GetHostName();

  public Task AddAsync(TemperatureReading reading, CancellationToken cancelToken = default) =>
    AddRangeAsync([reading,], cancelToken);

  public async Task AddRangeAsync(
    IEnumerable<TemperatureReading> readings,
    CancellationToken cancelToken = default
  )
  {
    InfluxConfiguration settings = influxConfiguration.Value;
    WriteApiAsync? writeApi = _influx.GetWriteApiAsync();

    List<PointData> points = readings.Select(
      r => PointData.Measurement("temperature")
        .Field("value", (double)r.Value)
        .Timestamp(r.AsOf, WritePrecision.Ms)
        .Tag("sensor", r.Sensor)
        .Tag("source", _hostname)
    ).ToList();

    await writeApi.WritePointsAsync(points, settings.Bucket, "ollimart.in", cancelToken);
  }

  public IEnumerable<TemperatureReading> GetAll() => throw new NotImplementedException();

  public void Dispose()
  {
    _influx.Dispose();
  }
}