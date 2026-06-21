import type { Device, SensorReading } from '../types';

const DEVICES_KEY = 'fleetmonitor:devices';
const DEVICES_CACHED_AT_KEY = 'fleetmonitor:devices:cachedAt';
const READINGS_KEY_PREFIX = 'fleetmonitor:readings:';

function readingsKey(deviceId: string): string {
  return `${READINGS_KEY_PREFIX}${deviceId}`;
}

export function saveDevicesCache(devices: Device[]): void {
  localStorage.setItem(DEVICES_KEY, JSON.stringify(devices));
  localStorage.setItem(DEVICES_CACHED_AT_KEY, new Date().toISOString());
}

export function loadDevicesCache(): Device[] {
  const raw = localStorage.getItem(DEVICES_KEY);
  if (!raw) {
    return [];
  }

  try {
    return JSON.parse(raw) as Device[];
  } catch {
    return [];
  }
}

export function getDevicesCacheTime(): string | null {
  return localStorage.getItem(DEVICES_CACHED_AT_KEY);
}

export function saveReadingsCache(deviceId: string, readings: SensorReading[]): void {
  localStorage.setItem(readingsKey(deviceId), JSON.stringify(readings));
}

export function loadReadingsCache(deviceId: string): SensorReading[] {
  const raw = localStorage.getItem(readingsKey(deviceId));
  if (!raw) {
    return [];
  }

  try {
    return JSON.parse(raw) as SensorReading[];
  } catch {
    return [];
  }
}

export function clearOfflineCache(): void {
  localStorage.removeItem(DEVICES_KEY);
  localStorage.removeItem(DEVICES_CACHED_AT_KEY);
  for (let i = localStorage.length - 1; i >= 0; i--) {
    const key = localStorage.key(i);
    if (key?.startsWith(READINGS_KEY_PREFIX)) {
      localStorage.removeItem(key);
    }
  }
}
