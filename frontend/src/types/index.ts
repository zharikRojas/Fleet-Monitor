export type UserRole = 'Admin' | 'Viewer';

export interface AuthUser {
  token: string;
  email: string;
  role: UserRole;
  expiresAt: string;
}
