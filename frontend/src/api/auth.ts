import { apiFetch } from './client';
import type { AuthUser, UserRole } from '../types';

interface LoginResponse {
  token: string;
  email: string;
  role: string;
  expiresAt: string;
}

export async function login(email: string, password: string): Promise<AuthUser> {
  const result = await apiFetch<LoginResponse>('/auth/login', {
    method: 'POST',
    body: JSON.stringify({ email, password }),
  });

  return {
    token: result.token,
    email: result.email,
    role: result.role as UserRole,
    expiresAt: result.expiresAt,
  };
}
