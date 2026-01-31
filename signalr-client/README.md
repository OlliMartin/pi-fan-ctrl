# @acaad/fan-control-signalr

TypeScript SignalR client for pi-fan-ctrl with full type safety and automatic code generation support.

## Installation

```bash
npm install @acaad/fan-control-signalr
```

## Features

- ✅ Full TypeScript support with generated types
- ✅ Type-safe SignalR client matching C# hub implementation
- ✅ Automatic reconnection support
- ✅ Input validation matching server-side validation
- ✅ Promise-based async API
- ✅ Event-driven architecture for real-time updates

## Quick Start

```typescript
import { FanControlClient } from '@acaad/fan-control-signalr';

// Create client
const client = new FanControlClient({
  hubUrl: 'http://localhost:8080',
  handlers: {
    onTemperatureUpdate: (update) => {
      console.log(`Temperature: ${update.value}°C from ${update.source}`);
    },
    onFanRpmUpdate: (update) => {
      console.log(`Fan RPM: ${update.value} from ${update.source}`);
    }
  }
});

// Connect
await client.start();

// Get current values
await client.getCurrentTemperature();

// Simulate temperature
await client.simulateTemperature(45.5);

// Set fan speed
await client.setFanSpeed(80);

// Reset to original settings
await client.resetFanSettings();

// Disconnect
await client.stop();
```

## API Reference

### FanControlClient

The main client class for interacting with the pi-fan-ctrl SignalR hub.

#### Constructor

```typescript
new FanControlClient(options: FanControlClientOptions)
```

**Options:**
- `hubUrl` (string, required): Base URL of the SignalR hub (e.g., "http://localhost:8080")
- `hubPath` (string, optional): Path to the hub endpoint (default: "/hubs/fancontrol")
- `automaticReconnect` (boolean, optional): Enable automatic reconnection (default: true)
- `logLevel` (LogLevel, optional): SignalR log level (default: Information)
- `handlers` (FanControlClientHandlers, optional): Event handlers for receiving messages

#### Methods

##### `start(): Promise<void>`
Start the SignalR connection.

##### `stop(): Promise<void>`
Stop the SignalR connection.

##### `getCurrentTemperature(): Promise<void>`
Request current temperature and RPM values. Updates are received via event handlers.

##### `simulateTemperature(temperature: number): Promise<void>`
Simulate a temperature reading.
- **Parameters:**
  - `temperature`: Temperature value in Celsius (-273.15 to 200)
- **Throws:** Error if temperature is out of range

##### `setFanSpeed(speedPercentage: number): Promise<void>`
Set fan speed directly.
- **Parameters:**
  - `speedPercentage`: Fan speed percentage (0 to 100)
- **Throws:** Error if speed is out of range

##### `resetFanSettings(): Promise<void>`
Reset fan settings to original configuration from appsettings.json.

#### Event Handlers

You can set event handlers either in the constructor or using the `on*` methods:

```typescript
client.onTemperatureUpdate((update) => {
  console.log(`Temp: ${update.value}°C`);
});

client.onFanRpmUpdate((update) => {
  console.log(`RPM: ${update.value}`);
});

client.onTemperatureSimulated((simulated) => {
  console.log(`Simulated: ${simulated.temperature}°C`);
});

client.onFanSpeedSet((speedSet) => {
  console.log(`Speed set to: ${speedSet.speedPercentage}%`);
});

client.onFanSettingsReset(() => {
  console.log('Settings reset');
});
```

#### Properties

##### `state: HubConnectionState`
Get the current connection state.

##### `connectionId: string | null`
Get the connection ID (available after connection is established).

## Data Types

### TemperatureUpdateDto

```typescript
interface TemperatureUpdateDto {
  source: string;      // Source of the reading (e.g., "Simulated", "Aggregate")
  value: number;       // Temperature in Celsius
  timestamp: Date;     // When the reading was taken
}
```

### FanRpmUpdateDto

```typescript
interface FanRpmUpdateDto {
  source: string;      // Source of the reading
  value: number;       // Fan speed in RPM
  timestamp: Date;     // When the reading was taken
}
```

### TemperatureSimulatedDto

```typescript
interface TemperatureSimulatedDto {
  temperature: number; // Simulated temperature value
}
```

### FanSpeedSetDto

```typescript
interface FanSpeedSetDto {
  speedPercentage: number; // Fan speed percentage (0-100)
}
```

### FanSettingsResetDto

```typescript
interface FanSettingsResetDto {
  // Empty DTO indicating settings were reset
}
```

## Factory Functions

For convenience, factory functions are provided:

```typescript
import { createFanControlClient, createDefaultClient } from '@acaad/fan-control-signalr';

// Create with custom options
const client1 = createFanControlClient({
  hubUrl: 'http://localhost:8080',
  automaticReconnect: true
});

// Create with defaults
const client2 = createDefaultClient('http://localhost:8080');
```

## Error Handling

The client validates inputs and throws errors for invalid values:

```typescript
try {
  await client.simulateTemperature(300); // Too high!
} catch (error) {
  console.error('Temperature must be between -273.15 and 200');
}

try {
  await client.setFanSpeed(150); // Too high!
} catch (error) {
  console.error('Speed percentage must be between 0 and 100');
}
```

## Integration Testing

The package includes comprehensive integration tests that verify compatibility with the C# pi-fan-ctrl hub.

To run integration tests:

```bash
# Start the pi-fan-ctrl application first
cd ../PiFanCtrl
NO_PWM=true NO_FAN_RPM=true NO_SENSORS=true NO_INFLUX=true dotnet run

# In another terminal, run the tests
cd signalr-client
npm test:integration
```

## Development

```bash
# Install dependencies
npm install

# Build
npm run build

# Run tests
npm test

# Run integration tests (requires running pi-fan-ctrl)
npm run test:integration
```

## License

MIT
