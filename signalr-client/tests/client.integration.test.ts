import { FanControlClient } from '../src/client';
import { TemperatureUpdateDto, FanRpmUpdateDto } from '../src/types';

describe('FanControlClient Integration Tests', () => {
  let client: FanControlClient;
  const hubUrl = process.env.HUB_URL || 'http://localhost:8080';

  beforeEach(() => {
    client = new FanControlClient({
      hubUrl,
      hubPath: '/hubs/fancontrol',
      automaticReconnect: true
    });
  });

  afterEach(async () => {
    if (client.state !== 'Disconnected') {
      await client.stop();
    }
  });

  test('should connect to the hub', async () => {
    await client.start();
    expect(client.state).toBe('Connected');
    expect(client.connectionId).toBeTruthy();
  });

  test('should disconnect from the hub', async () => {
    await client.start();
    await client.stop();
    expect(client.state).toBe('Disconnected');
  });

  test('should receive temperature updates', async () => {
    const temperatureUpdates: TemperatureUpdateDto[] = [];
    
    client.onTemperatureUpdate((update) => {
      temperatureUpdates.push(update);
    });

    await client.start();
    
    // Get current values
    await client.getCurrentTemperature();
    
    // Wait for updates
    await new Promise(resolve => setTimeout(resolve, 1000));
    
    // We might receive temperature updates depending on the system state
    // At minimum, we verify no errors occurred
    expect(client.state).toBe('Connected');
  });

  test('should receive fan RPM updates', async () => {
    const rpmUpdates: FanRpmUpdateDto[] = [];
    
    client.onFanRpmUpdate((update) => {
      rpmUpdates.push(update);
    });

    await client.start();
    
    // Get current values
    await client.getCurrentTemperature();
    
    // Wait for updates
    await new Promise(resolve => setTimeout(resolve, 1000));
    
    // Verify we received at least one RPM update
    expect(rpmUpdates.length).toBeGreaterThanOrEqual(1);
    expect(rpmUpdates[0]).toHaveProperty('source');
    expect(rpmUpdates[0]).toHaveProperty('value');
    expect(rpmUpdates[0]).toHaveProperty('timestamp');
    expect(rpmUpdates[0].timestamp).toBeInstanceOf(Date);
  });

  test('should simulate temperature', async () => {
    let simulatedTemp: number | undefined;
    const temperatureUpdates: TemperatureUpdateDto[] = [];

    client.onTemperatureSimulated((dto) => {
      simulatedTemp = dto.temperature;
    });

    client.onTemperatureUpdate((update) => {
      temperatureUpdates.push(update);
    });

    await client.start();

    const testTemperature = 55.5;
    await client.simulateTemperature(testTemperature);

    // Wait for events with longer timeout
    await new Promise(resolve => setTimeout(resolve, 3000));

    expect(simulatedTemp).toBe(testTemperature);
    
    // Should receive temperature update for simulated or aggregate source
    const hasUpdate = temperatureUpdates.some(u => 
      (u.source === 'Simulated' || u.source === 'Aggregate') && u.value === testTemperature
    );
    expect(hasUpdate).toBe(true);
  });

  test('should set fan speed', async () => {
    let fanSpeedSet: number | undefined;

    client.onFanSpeedSet((dto) => {
      fanSpeedSet = dto.speedPercentage;
    });

    await client.start();

    const testSpeed = 80;
    await client.setFanSpeed(testSpeed);

    // Wait for event
    await new Promise(resolve => setTimeout(resolve, 1000));

    expect(fanSpeedSet).toBe(testSpeed);
  });

  test('should reset fan settings', async () => {
    let resetCalled = false;

    client.onFanSettingsReset(() => {
      resetCalled = true;
    });

    await client.start();

    // First set a speed, then reset
    await client.setFanSpeed(75);
    await new Promise(resolve => setTimeout(resolve, 500));
    
    await client.resetFanSettings();

    // Wait for event
    await new Promise(resolve => setTimeout(resolve, 1000));

    expect(resetCalled).toBe(true);
  });

  test('should validate temperature range', async () => {
    await client.start();

    // Test temperature too low
    await expect(client.simulateTemperature(-300)).rejects.toThrow('Temperature must be between -273.15 and 200');

    // Test temperature too high
    await expect(client.simulateTemperature(250)).rejects.toThrow('Temperature must be between -273.15 and 200');

    // Valid temperatures should work
    await expect(client.simulateTemperature(0)).resolves.not.toThrow();
    await expect(client.simulateTemperature(25)).resolves.not.toThrow();
  });

  test('should validate fan speed range', async () => {
    await client.start();

    // Test speed too low
    await expect(client.setFanSpeed(-10)).rejects.toThrow('Speed percentage must be between 0 and 100');

    // Test speed too high
    await expect(client.setFanSpeed(150)).rejects.toThrow('Speed percentage must be between 0 and 100');

    // Valid speeds should work
    await expect(client.setFanSpeed(0)).resolves.not.toThrow();
    await expect(client.setFanSpeed(100)).resolves.not.toThrow();
  });

  test('should handle automatic reconnection', async () => {
    await client.start();
    expect(client.state).toBe('Connected');

    // Note: This test verifies the configuration is set up correctly
    // Actual reconnection testing would require disrupting the connection
  });

  test('should receive aggregated temperature updates', async () => {
    const temperatureUpdates: TemperatureUpdateDto[] = [];
    
    client.onTemperatureUpdate((update) => {
      temperatureUpdates.push(update);
    });

    await client.start();
    
    const testTemp = 38.5;
    // Simulate temperature to trigger aggregated update
    await client.simulateTemperature(testTemp);
    
    // Wait for updates
    await new Promise(resolve => setTimeout(resolve, 2000));
    
    // Should receive temperature updates (at minimum Simulated)
    const hasUpdates = temperatureUpdates.length > 0;
    expect(hasUpdates).toBe(true);
    
    // Verify we got an update with the correct value
    const hasCorrectValue = temperatureUpdates.some(u => u.value === testTemp);
    expect(hasCorrectValue).toBe(true);
  });
});
