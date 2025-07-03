import { api } from './api';
import { Point, CreatePointDto, UpdatePointDto, ApiResponse } from '../types';

export const pointService = {
  async getAll(): Promise<Point[]> {
    const response = await api.get<ApiResponse<Point[]>>('/points');
    return response.data.data || [];
  },

  async getById(id: number): Promise<Point | null> {
    try {
      const response = await api.get<ApiResponse<Point>>(`/points/${id}`);
      return response.data.data;
    } catch (error) {
      console.error('Error fetching point:', error);
      return null;
    }
  },

  async create(point: CreatePointDto): Promise<Point | null> {
    try {
      const response = await api.post<ApiResponse<Point>>('/points', point);
      return response.data.data;
    } catch (error) {
      console.error('Error creating point:', error);
      return null;
    }
  },

  async update(id: number, point: UpdatePointDto): Promise<Point | null> {
    try {
      console.log('🔹 PointService.update called:', { id, point });
      const response = await api.put<ApiResponse<Point>>(`/points/${id}`, point);
      console.log('🔹 PointService.update response:', response.data);
      return response.data.data;
    } catch (error) {
      console.error('🔹 PointService.update error:', error);
      console.error('🔹 Error response:', error.response?.data);
      return null;
    }
  },

  async delete(id: number): Promise<boolean> {
    try {
      const response = await api.delete<ApiResponse<boolean>>(`/points/${id}`);
      return response.data.success;
    } catch (error) {
      console.error('Error deleting point:', error);
      return false;
    }
  }
}; 