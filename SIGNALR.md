# SignalR Interface for Pi Fan Control

This document describes the SignalR interface for interacting with the Pi Fan Control system.

## Overview

The SignalR hub provides a real-time, push-based interface for:
- Reading current aggregated temperature and fan RPM values
- Simulating temperature values for testing
- Setting fan speed directly
- Resetting fan settings to original configuration

## Connection

**Hub URL**: `/hubs/fancontrol`

Example connection (JavaScript):
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/fancontrol")
    .withAutomaticReconnect()
    .build();

await connection.start();
```

## Server-to-Client Events (Push Notifications)

The hub automatically pushes updates to all connected clients when values change:

### TemperatureUpdate
Sent when a new temperature reading is available.

**Payload**:
```json
{
  "source": "string",      // Source of the temperature reading (e.g., "Simulated", "BMP280")
  "value": 45.5,          // Temperature value in °C
  "timestamp": "2026-01-30T17:40:55.345Z"  // ISO 8601 timestamp
}
```

**Example Handler** (JavaScript):
```javascript
connection.on("TemperatureUpdate", (data) => {
    console.log(`Temperature from ${data.source}: ${data.value}°C`);
});
```

### FanRpmUpdate
Sent when a new fan RPM reading is available.

**Payload**:
```json
{
  "source": "string",      // Source of the RPM reading
  "value": 1234,          // Fan speed in RPM
  "timestamp": "2026-01-30T17:40:55.345Z"  // ISO 8601 timestamp
}
```

**Example Handler** (JavaScript):
```javascript
connection.on("FanRpmUpdate", (data) => {
    console.log(`Fan RPM from ${data.source}: ${data.value} RPM`);
});
```

### TemperatureSimulated
Sent when a temperature value is simulated via the `SimulateTemperature` method.

**Payload**: `number` - The simulated temperature value

**Example Handler** (JavaScript):
```javascript
connection.on("TemperatureSimulated", (temperature) => {
    console.log(`Temperature simulated: ${temperature}°C`);
});
```

### FanSpeedSet
Sent when fan speed is set directly via the `SetFanSpeed` method.

**Payload**: `number` - The fan speed percentage (0-100)

**Example Handler** (JavaScript):
```javascript
connection.on("FanSpeedSet", (speed) => {
    console.log(`Fan speed set to: ${speed}%`);
});
```

### FanSettingsReset
Sent when fan settings are reset to original configuration.

**Payload**: None

**Example Handler** (JavaScript):
```javascript
connection.on("FanSettingsReset", () => {
    console.log("Fan settings reset");
});
```

## Client-to-Server Methods

These methods can be invoked by clients to control the fan system:

### GetCurrentTemperature()
Requests the hub to send the current temperature and RPM values.

**Parameters**: None

**Returns**: `Promise<void>`

**Example** (JavaScript):
```javascript
await connection.invoke("GetCurrentTemperature");
```

### SimulateTemperature(temperature)
Simulates a temperature reading for testing purposes.

**Parameters**:
- `temperature` (decimal): Temperature value in °C

**Returns**: `Promise<void>`

**Example** (JavaScript):
```javascript
await connection.invoke("SimulateTemperature", 45.5);
```

### SetFanSpeed(speedPercentage)
Sets the fan speed directly by manipulating the `FanSettings` (sets both `MinimumSpeed` and `PanicSpeed` to the same value).

**Parameters**:
- `speedPercentage` (decimal): Fan speed as a percentage (0-100)

**Returns**: `Promise<void>`

**Example** (JavaScript):
```javascript
await connection.invoke("SetFanSpeed", 80);
```

### ResetFanSettings()
Resets fan settings to the original configuration from `appsettings.json` and clears any temperature simulation.

**Parameters**: None

**Returns**: `Promise<void>`

**Example** (JavaScript):
```javascript
await connection.invoke("ResetFanSettings");
```

## Complete Example (JavaScript)

```javascript
const signalR = require("@microsoft/signalr");

async function main() {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:8080/hubs/fancontrol")
        .withAutomaticReconnect()
        .build();

    // Register event handlers
    connection.on("TemperatureUpdate", (data) => {
        console.log(`Temperature: ${data.value}°C from ${data.source}`);
    });

    connection.on("FanRpmUpdate", (data) => {
        console.log(`Fan RPM: ${data.value} from ${data.source}`);
    });

    connection.on("TemperatureSimulated", (temp) => {
        console.log(`Simulated: ${temp}°C`);
    });

    connection.on("FanSpeedSet", (speed) => {
        console.log(`Fan speed set to: ${speed}%`);
    });

    connection.on("FanSettingsReset", () => {
        console.log("Settings reset");
    });

    // Connect
    await connection.start();
    console.log("Connected!");

    // Get current values
    await connection.invoke("GetCurrentTemperature");

    // Simulate temperature
    await connection.invoke("SimulateTemperature", 50);

    // Set fan speed
    await connection.invoke("SetFanSpeed", 75);

    // Reset to original settings
    await connection.invoke("ResetFanSettings");

    // Disconnect
    await connection.stop();
}

main();
```

## Testing

A test HTML client is available at `/signalr-test.html` when running the application. This client provides a UI for testing all SignalR functionality.

## Configuration

The fan settings can be configured in `appsettings.json` under the `FanSettings` section:

```json
{
  "FanSettings": {
    "MinimumSpeedTemperature": 25,
    "MinimumSpeed": 40,
    "PanicFromTemperature": 55,
    "PanicSpeed": 100,
    "CurvePoints": [
      {
        "Active": true,
        "Temperature": 40,
        "FanPercentage": 80
      }
    ]
  }
}
```

These settings are loaded at startup and can be restored using the `ResetFanSettings()` method.

## Development

For development/testing on systems without GPIO hardware, use the following environment variables:

```bash
NO_PWM=true         # Use dummy PWM controller instead of GPIO
NO_FAN_RPM=true     # Use dummy RPM sensor instead of GPIO
NO_SENSORS=true     # Disable hardware sensors
NO_INFLUX=true      # Disable InfluxDB integration
```

Example:
```bash
NO_PWM=true NO_FAN_RPM=true NO_SENSORS=true NO_INFLUX=true dotnet run
```
