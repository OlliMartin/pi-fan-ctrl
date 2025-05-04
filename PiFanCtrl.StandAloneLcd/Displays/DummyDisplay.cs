using Microsoft.Extensions.Logging;
using PiFanControl.Abstractions;
using PiFanCtrl.StandAloneLcd.Interfaces;

namespace PiFanCtrl.StandAloneLcd.Displays;

public class DummyDisplay : IDisplay
{
  private readonly ILogger<DummyDisplay> _logger;

  public DummyDisplay(ILogger<DummyDisplay> logger)
  {
    _logger = logger;
  }

  public Task Draw(SystemInfo systemInfo, CancellationToken cancelToken = default)
  {
    _logger.LogInformation(
      "Received sys-info: Temp: {temp} | Pwm: {pwm} | Fan-Rpm: {fanRpm} | AsOf: {asOf}",
      systemInfo.MeasuredTemperature,
      systemInfo.PwmPercentage,
      systemInfo.MeasuredFanRpm,
      systemInfo.AsOf
    );

    return Task.CompletedTask;
  }
}