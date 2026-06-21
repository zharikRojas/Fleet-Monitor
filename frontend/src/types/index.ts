export type UserRole = 'Admin' | 'Viewer';

export interface AuthUser {
  token: string;
  email: string;
  role: UserRole;
  expiresAt: string;
}

export interface Device {
  id: string;
  name: string;
  lastLat: number;
  lastLng: number;
  fuelLevel: number;
  fuelConsumptionRate: number;
  temperature: number;
  speed: number;
  updatedAt: string;
}

export interface SensorReading {
  timestamp: string;
  lat: number;
  lng: number;
  fuel: number;
  temperature: number;
  speed: number;
}

export interface Alert {
  id: string;
  deviceId: string;
  type: 'LowFuel';
  estimatedMinutesRemaining: number;
  createdAt: string;
  acknowledged: boolean;
  device: Device;
}
