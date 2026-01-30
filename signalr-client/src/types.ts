/**
 * Data Transfer Objects (DTOs) matching the C# SignalR hub
 */

export interface TemperatureUpdateDto {
  source: string;
  value: number;
  timestamp: Date;
}

export interface FanRpmUpdateDto {
  source: string;
  value: number;
  timestamp: Date;
}

export interface TemperatureSimulatedDto {
  temperature: number;
}

export interface FanSpeedSetDto {
  speedPercentage: number;
}

export interface FanSettingsResetDto {
  // Empty DTO as per C# implementation
}
