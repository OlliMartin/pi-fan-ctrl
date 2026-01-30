using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using JetBrains.Annotations;
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
  private InfluxDBClient? _influx;
  private DateTime _clientCreatedAt;

  public InfluxReadingStore(
    ILogger<InfluxReadingStore> logger,
    IOptions<InfluxConfiguration> influxConfiguration
  )
  {
    _logger = logger;
    _influxConfiguration = influxConfiguration;
  }

  [MemberNotNull(nameof(_influx))]
  private void GetOrCreateClient()
  {
    InfluxConfiguration settings = _influxConfiguration.Value;

    if (_influx is not null && _clientCreatedAt + settings.RenewClientAfter > DateTime.UtcNow)
    {
      return;
    }

    _logger.LogInformation("(Re)Newing influx db client.");

    InfluxDBClient newClient = new(
      settings.Url,
      settings.Password
    );

    _clientCreatedAt = DateTime.UtcNow;

    InfluxDBClient? oldClient = Interlocked.Exchange(ref _influx, newClient);

    oldClient?.Dispose();
  }

  private readonly string _hostname = System.Net.Dns.GetHostName();

  public Task AddAsync(IReading reading, CancellationToken cancelToken = default) =>
    AddRangeAsync([reading,], cancelToken);

  public async Task AddRangeAsync(
    IEnumerable<IReading> readings,
    CancellationToken cancelToken = default
  )
  {
    try
    {
      InfluxConfiguration settings = _influxConfiguration.Value;

      GetOrCreateClient();
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
    catch (Exception ex)
    {
      _logger.LogError(ex, "An error occurred persisting data in influx.");
    }
  }

  public IEnumerable<IReading> GetAll() => throw new NotImplementedException();

  public decimal? GetLatest(string source) => null; // Influx store doesn't support this operation

  public event EventHandler<ReadingChangedEventArgs>? ReadingChanged;

  public void Dispose()
  {
    Stopwatch sw = Stopwatch.StartNew();
    _logger.LogInformation("Disposing {name}.", nameof(InfluxReadingStore));

    _influx.Dispose();

    sw.Stop();
    _logger.LogDebug("{name} disposed in {elapsed}.", nameof(InfluxReadingStore), sw.Elapsed);
  }
}