using System.Diagnostics;
using System.Globalization;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Options;
using PiFanCtrl.Interfaces;
using PiFanCtrl.Model;
using PiFanCtrl.Model.Settings;

namespace PiFanCtrl.Services.Stores;

public sealed class InfluxReadingStore
  : IReadingStore, IDisposable
{
  private readonly ILogger _logger;
  private readonly IOptions<InfluxConfiguration> _influxConfiguration;
  private readonly InfluxDBClient _influx;

  public InfluxReadingStore(
    ILogger<InfluxReadingStore> logger,
    IOptions<InfluxConfiguration> influxConfiguration
  )
  {
    _logger = logger;
    _influxConfiguration = influxConfiguration;
    InfluxConfiguration settings = influxConfiguration.Value;

    _influx = new(
      settings.Url,
      settings.Password
    );
  }

  private readonly string _hostname = System.Net.Dns.GetHostName();

  public Task AddAsync(IReading reading, CancellationToken cancelToken = default) =>
    AddRangeAsync([reading,], cancelToken);

  public async Task AddRangeAsync(
    IEnumerable<IReading> readings,
    CancellationToken cancelToken = default
  )
  {
    InfluxConfiguration settings = _influxConfiguration.Value;
    WriteApiAsync? writeApi = _influx.GetWriteApiAsync();

    List<PointData> points = readings.Select(
      r => PointData.Measurement(r.Measurement)
        .Field("value", (double)r.Value)
        .Timestamp(r.AsOf, WritePrecision.Ms)
        .Tag("source", r.Source)
        .Tag("host", _hostname)
    ).ToList();

    await writeApi.WritePointsAsync(points, settings.Bucket, settings.Organisation, cancelToken);
  }

  public IEnumerable<IReading> GetAll() => throw new NotImplementedException();

  public void Dispose()
  {
    Stopwatch sw = Stopwatch.StartNew();
    _logger.LogInformation("Disposing {name}.", nameof(InfluxReadingStore));

    _influx.Dispose();

    sw.Stop();
    _logger.LogDebug("{name} disposed in {elapsed}.", nameof(InfluxReadingStore), sw.Elapsed);
  }
}