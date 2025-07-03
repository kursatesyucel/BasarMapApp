import { api } from './api';
import { Polygon, CreatePolygonDto, UpdatePolygonDto, ApiResponse } from '../types';

export const polygonService = {
  async getAll(): Promise<Polygon[]> {
    const response = await api.get<ApiResponse<Polygon[]>>('/polygons');
    return response.data.data || [];
  },

  async getById(id: number): Promise<Polygon | null> {
    try {
      const response = await api.get<ApiResponse<Polygon>>(`/polygons/${id}`);
      return response.data.data;
    } catch (error) {
      console.error('Error fetching polygon:', error);
      return null;
    }
  },

  async create(polygon: CreatePolygonDto): Promise<Polygon | null> {
    try {
      const response = await api.post<ApiResponse<Polygon>>('/polygons', polygon);
      return response.data.data;
    } catch (error) {
      console.error('Error creating polygon:', error);
      return null;
    }
  },

  async createWithIntersectionHandling(polygon: CreatePolygonDto): Promise<Polygon | null> {
    try {
      const response = await api.post<ApiResponse<Polygon>>('/polygons/create-with-intersection-handling', polygon);
      return response.data.data;
    } catch (error) {
      console.error('Error creating polygon with intersection handling:', error);
      return null;
    }
  },

  async update(id: number, polygon: UpdatePolygonDto): Promise<Polygon | null> {
    try {
      const response = await api.put<ApiResponse<Polygon>>(`/polygons/${id}`, polygon);
      return response.data.data;
    } catch (error) {
      console.error('Error updating polygon:', error);
      return null;
    }
  },

  async delete(id: number): Promise<boolean> {
    try {
      const response = await api.delete<ApiResponse<boolean>>(`/polygons/${id}`);
      return response.data.success;
    } catch (error) {
      console.error('Error deleting polygon:', error);
      return false;
    }
  }
}; 