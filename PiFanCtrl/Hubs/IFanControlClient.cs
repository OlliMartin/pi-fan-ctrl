using PiFanCtrl.Model.SignalR;

namespace PiFanCtrl.Hubs;

/// <summary>
/// Interface for strongly-typed SignalR client methods
/// </summary>
public interface IFanControlClient
{
  Task TemperatureUpdate(TemperatureUpdateDto update);
  Task FanRpmUpdate(FanRpmUpdateDto update);
  Task TemperatureSimulated(TemperatureSimulatedDto simulated);
  Task FanSpeedSet(FanSpeedSetDto speedSet);
  Task FanSettingsReset(FanSettingsResetDto reset);
}
