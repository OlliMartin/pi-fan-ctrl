using PiFanControl.Abstractions;
using PiFanCtrl.Services.Stores;

namespace PiFanCtrl.Services;

public class SystemInfoProvider
{
  private readonly SlidingReadingStore _readingStore;

  public SystemInfoProvider(SlidingReadingStore readingStore)
  {
    _readingStore = readingStore;
  }

  public Task<SystemInfo> GetLatestSysInfoAsync(CancellationToken cancelToken = default)
  {
    decimal aggrTemp = _readingStore.GetLatest("Aggregate")?.Value ?? -1;
    decimal measuredTemp = _readingStore.GetLatest("BMP280-Bus-1-Addr-118")?.Value ?? -1;

    decimal rpm = _readingStore.GetLatest("RPM-Pin-26")?.Value ?? -1;
    decimal pwm = _readingStore.GetLatest("Calculated")?.Value ?? -1;

    return Task.FromResult(
      new SystemInfo()
      {
        AggregatedTemperature = aggrTemp,
        AsOf = DateTime.UtcNow,
        MeasuredTemperature = measuredTemp,
        PwmPercentage = pwm,
        MeasuredFanRpm = rpm
      }
    );
  }
}