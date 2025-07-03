import { api } from './api';
import { Line, CreateLineDto, UpdateLineDto, ApiResponse } from '../types';

export const lineService = {
  async getAll(): Promise<Line[]> {
    const response = await api.get<ApiResponse<Line[]>>('/lines');
    return response.data.data || [];
  },

  async getById(id: number): Promise<Line | null> {
    try {
      const response = await api.get<ApiResponse<Line>>(`/lines/${id}`);
      return response.data.data;
    } catch (error) {
      console.error('Error fetching line:', error);
      return null;
    }
  },

  async create(line: CreateLineDto): Promise<Line | null> {
    try {
      console.log('LineService.create called with:', line);
      const response = await api.post<ApiResponse<Line>>('/lines', line);
      console.log('LineService.create response:', response.data);
      return response.data.data;
    } catch (error) {
      console.error('Error creating line:', error);
      console.error('Error details:', error.response?.data);
      return null;
    }
  },

  async update(id: number, line: UpdateLineDto): Promise<Line | null> {
    try {
      console.log('📏 LineService.update called:', { id, line });
      const response = await api.put<ApiResponse<Line>>(`/lines/${id}`, line);
      console.log('📏 LineService.update response:', response.data);
      return response.data.data;
    } catch (error) {
      console.error('📏 LineService.update error:', error);
      console.error('📏 Error response:', error.response?.data);
      return null;
    }
  },

  async delete(id: number): Promise<boolean> {
    try {
      const response = await api.delete<ApiResponse<boolean>>(`/lines/${id}`);
      return response.data.success;
    } catch (error) {
      console.error('Error deleting line:', error);
      return false;
    }
  }
}; 