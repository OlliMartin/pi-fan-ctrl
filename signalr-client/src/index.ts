import { FanControlClient, FanControlClientOptions } from './client';
import {
  TemperatureUpdateDto,
  FanRpmUpdateDto,
  TemperatureSimulatedDto,
  FanSpeedSetDto,
  FanSettingsResetDto
} from './types';

/**
 * Factory function to create a FanControlClient instance
 */
export function createFanControlClient(options: FanControlClientOptions): FanControlClient {
  return new FanControlClient(options);
}

/**
 * Factory function to create a FanControlClient with default settings
 */
export function createDefaultClient(hubUrl: string): FanControlClient {
  return new FanControlClient({
    hubUrl,
    automaticReconnect: true
  });
}

// Export everything
export {
  FanControlClient,
  FanControlClientOptions,
  TemperatureUpdateDto,
  FanRpmUpdateDto,
  TemperatureSimulatedDto,
  FanSpeedSetDto,
  FanSettingsResetDto
};

export * from './client';
export * from './types';
