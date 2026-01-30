import * as signalR from '@microsoft/signalr';
import {
  TemperatureUpdateDto,
  FanRpmUpdateDto,
  TemperatureSimulatedDto,
  FanSpeedSetDto,
  FanSettingsResetDto
} from './types';

/**
 * Client event handlers for receiving messages from the SignalR hub
 */
export interface FanControlClientHandlers {
  onTemperatureUpdate?: (update: TemperatureUpdateDto) => void | Promise<void>;
  onFanRpmUpdate?: (update: FanRpmUpdateDto) => void | Promise<void>;
  onTemperatureSimulated?: (simulated: TemperatureSimulatedDto) => void | Promise<void>;
  onFanSpeedSet?: (speedSet: FanSpeedSetDto) => void | Promise<void>;
  onFanSettingsReset?: (reset: FanSettingsResetDto) => void | Promise<void>;
}

/**
 * Configuration options for the FanControlClient
 */
export interface FanControlClientOptions {
  /**
   * Base URL of the SignalR hub (e.g., "http://localhost:8080")
   */
  hubUrl: string;

  /**
   * Optional path to the hub endpoint (default: "/hubs/fancontrol")
   */
  hubPath?: string;

  /**
   * Enable automatic reconnection (default: true)
   */
  automaticReconnect?: boolean;

  /**
   * SignalR log level (default: Information)
   */
  logLevel?: signalR.LogLevel;

  /**
   * Event handlers for receiving messages from the hub
   */
  handlers?: FanControlClientHandlers;
}

/**
 * Type-safe SignalR client for pi-fan-ctrl
 */
export class FanControlClient {
  private connection: signalR.HubConnection;
  private handlers: FanControlClientHandlers;

  constructor(options: FanControlClientOptions) {
    const hubPath = options.hubPath || '/hubs/fancontrol';
    const url = `${options.hubUrl}${hubPath}`;

    const builder = new signalR.HubConnectionBuilder()
      .withUrl(url)
      .configureLogging(options.logLevel ?? signalR.LogLevel.Information);

    if (options.automaticReconnect !== false) {
      builder.withAutomaticReconnect();
    }

    this.connection = builder.build();
    this.handlers = options.handlers || {};

    this.registerHandlers();
  }

  /**
   * Register all event handlers with the SignalR connection
   */
  private registerHandlers(): void {
    this.connection.on('TemperatureUpdate', (dto: TemperatureUpdateDto) => {
      // Convert timestamp string to Date object
      const update = {
        ...dto,
        timestamp: new Date(dto.timestamp)
      };
      this.handlers.onTemperatureUpdate?.(update);
    });

    this.connection.on('FanRpmUpdate', (dto: FanRpmUpdateDto) => {
      // Convert timestamp string to Date object
      const update = {
        ...dto,
        timestamp: new Date(dto.timestamp)
      };
      this.handlers.onFanRpmUpdate?.(update);
    });

    this.connection.on('TemperatureSimulated', (dto: TemperatureSimulatedDto) => {
      this.handlers.onTemperatureSimulated?.(dto);
    });

    this.connection.on('FanSpeedSet', (dto: FanSpeedSetDto) => {
      this.handlers.onFanSpeedSet?.(dto);
    });

    this.connection.on('FanSettingsReset', (dto: FanSettingsResetDto) => {
      this.handlers.onFanSettingsReset?.(dto);
    });
  }

  /**
   * Start the SignalR connection
   */
  async start(): Promise<void> {
    await this.connection.start();
  }

  /**
   * Stop the SignalR connection
   */
  async stop(): Promise<void> {
    await this.connection.stop();
  }

  /**
   * Get current temperature and RPM values
   */
  async getCurrentTemperature(): Promise<void> {
    await this.connection.invoke('GetCurrentTemperature');
  }

  /**
   * Simulate a temperature reading
   * @param temperature - Temperature value in Celsius (-273.15 to 200)
   */
  async simulateTemperature(temperature: number): Promise<void> {
    if (temperature < -273.15 || temperature > 200) {
      throw new Error('Temperature must be between -273.15 and 200');
    }
    await this.connection.invoke('SimulateTemperature', temperature);
  }

  /**
   * Set fan speed directly
   * @param speedPercentage - Fan speed percentage (0 to 100)
   */
  async setFanSpeed(speedPercentage: number): Promise<void> {
    if (speedPercentage < 0 || speedPercentage > 100) {
      throw new Error('Speed percentage must be between 0 and 100');
    }
    await this.connection.invoke('SetFanSpeed', speedPercentage);
  }

  /**
   * Reset fan settings to original configuration
   */
  async resetFanSettings(): Promise<void> {
    await this.connection.invoke('ResetFanSettings');
  }

  /**
   * Add a handler for temperature updates
   */
  onTemperatureUpdate(handler: (update: TemperatureUpdateDto) => void | Promise<void>): void {
    this.handlers.onTemperatureUpdate = handler;
  }

  /**
   * Add a handler for fan RPM updates
   */
  onFanRpmUpdate(handler: (update: FanRpmUpdateDto) => void | Promise<void>): void {
    this.handlers.onFanRpmUpdate = handler;
  }

  /**
   * Add a handler for temperature simulation events
   */
  onTemperatureSimulated(handler: (simulated: TemperatureSimulatedDto) => void | Promise<void>): void {
    this.handlers.onTemperatureSimulated = handler;
  }

  /**
   * Add a handler for fan speed set events
   */
  onFanSpeedSet(handler: (speedSet: FanSpeedSetDto) => void | Promise<void>): void {
    this.handlers.onFanSpeedSet = handler;
  }

  /**
   * Add a handler for fan settings reset events
   */
  onFanSettingsReset(handler: (reset: FanSettingsResetDto) => void | Promise<void>): void {
    this.handlers.onFanSettingsReset = handler;
  }

  /**
   * Get the underlying SignalR connection state
   */
  get state(): signalR.HubConnectionState {
    return this.connection.state;
  }

  /**
   * Get the connection ID (available after connection is established)
   */
  get connectionId(): string | null {
    return this.connection.connectionId;
  }
}
