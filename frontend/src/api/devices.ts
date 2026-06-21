import { apiFetch } from './client';
import type { Device, SensorReading } from '../types';

export function fetchDevices(token: string): Promise<Device[]> {
  return apiFetch<Device[]>('/devices', {}, token);
}

export function fetchDeviceReadings(
  token: string,
  deviceId: string,
): Promise<SensorReading[]> {
  return apiFetch<SensorReading[]>(`/devices/${deviceId}/readings`, {}, token);
}
