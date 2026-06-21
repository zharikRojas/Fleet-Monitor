import { apiFetch } from './client';
import type { Alert } from '../types';

export function fetchAlerts(token: string): Promise<Alert[]> {
  return apiFetch<Alert[]>('/alerts', {}, token);
}
