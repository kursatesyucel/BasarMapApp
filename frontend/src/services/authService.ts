import { api } from './api';
import { LoginRequest, RegisterRequest, AuthResponse, UserListItem, UpdateUserRoleRequest } from '../types';

export const authService = {
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    const response = await api.post('/auth/login', credentials);
    return response.data.data;
  },

  async register(credentials: RegisterRequest): Promise<AuthResponse> {
    const response = await api.post('/auth/register', credentials);
    return response.data.data;
  },

  async getAllUsers(): Promise<UserListItem[]> {
    const response = await api.get('/auth/users');
    return response.data.data;
  },

  async updateUserRole(userId: number, roleData: UpdateUserRoleRequest): Promise<UserListItem> {
    const response = await api.put(`/auth/users/${userId}/role`, roleData);
    return response.data.data;
  },

  // Local storage operations
  saveAuth(token: string, username: string, role: string): void {
    localStorage.setItem('token', token);
    localStorage.setItem('user', JSON.stringify({ username, role }));
  },

  clearAuth(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },

  getToken(): string | null {
    return localStorage.getItem('token');
  },

  getUser(): { username: string; role: string } | null {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  },

  isAuthenticated(): boolean {
    return !!this.getToken();
  }
};
