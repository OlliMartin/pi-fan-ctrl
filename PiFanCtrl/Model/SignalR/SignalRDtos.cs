namespace PiFanCtrl.Model.SignalR;

public record TemperatureUpdateDto(string Source, decimal Value, DateTime Timestamp);

public record FanRpmUpdateDto(string Source, decimal Value, DateTime Timestamp);

public record TemperatureSimulatedDto(decimal Temperature);

public record FanSpeedSetDto(decimal SpeedPercentage);

public record FanSettingsResetDto();
