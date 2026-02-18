import { api } from './api';
import type { UserDevice } from '../types/deviceTypes';

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

export const deviceService = {
  async getMyDevices(): Promise<UserDevice[]> {
    const response = await api.get<ApiResponse<UserDevice[]>>('/devices');
    return response.data.data;
  },

  async revokeDevice(deviceId: string): Promise<string> {
    const response = await api.delete<ApiResponse<string>>(`/devices/${deviceId}`);
    return response.data.message;
  },

  async setTrustedDevice(deviceId: string): Promise<string> {
    const response = await api.patch<ApiResponse<string>>(`/devices/${deviceId}/trusted`);
    return response.data.message;
  }
};
