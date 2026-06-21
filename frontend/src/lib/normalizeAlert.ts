import type { Alert, Device } from '../types';

type RawAlert = Record<string, unknown>;

function normalizeDevice(raw: unknown): Device {
  const d = (raw ?? {}) as Record<string, unknown>;
  return {
    id: String(d.id ?? d.Id ?? ''),
    name: String(d.name ?? d.Name ?? ''),
    lastLat: Number(d.lastLat ?? d.LastLat ?? 0),
    lastLng: Number(d.lastLng ?? d.LastLng ?? 0),
    fuelLevel: Number(d.fuelLevel ?? d.FuelLevel ?? 0),
    fuelConsumptionRate: Number(d.fuelConsumptionRate ?? d.FuelConsumptionRate ?? 0),
    temperature: Number(d.temperature ?? d.Temperature ?? 0),
    speed: Number(d.speed ?? d.Speed ?? 0),
    updatedAt: String(d.updatedAt ?? d.UpdatedAt ?? ''),
  };
}

export function normalizeAlert(raw: RawAlert): Alert {
  return {
    id: String(raw.id ?? raw.Id ?? ''),
    deviceId: String(raw.deviceId ?? raw.DeviceId ?? ''),
    type: (raw.type ?? raw.Type ?? 'LowFuel') as Alert['type'],
    estimatedMinutesRemaining: Number(
      raw.estimatedMinutesRemaining ?? raw.EstimatedMinutesRemaining ?? 0,
    ),
    createdAt: String(raw.createdAt ?? raw.CreatedAt ?? ''),
    acknowledged: Boolean(raw.acknowledged ?? raw.Acknowledged ?? false),
    device: normalizeDevice(raw.device ?? raw.Device),
  };
}
