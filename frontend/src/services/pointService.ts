import { api } from './api';
import { Point, CreatePointDto, UpdatePointDto } from '../types';

export const pointService = {
  async getAll(): Promise<Point[]> {
    const response = await api.get('/points');
    return response.data.data;
  },

  async getById(id: number): Promise<Point | null> {
    try {
      const response = await api.get(`/points/${id}`);
      return response.data.data;
    } catch (error: any) {
      if (error.response?.status === 404) {
        return null;
      }
      throw error;
    }
  },

  async create(point: CreatePointDto): Promise<Point> {
    const response = await api.post('/points', point);
    return response.data.data;
  },

  async update(id: number, point: UpdatePointDto): Promise<Point> {
    const response = await api.put(`/points/${id}`, point);
    return response.data.data;
  },

  async delete(id: number): Promise<void> {
    await api.delete(`/points/${id}`);
  },

  getPointsWithinPolygon: async (polygonCoordinates: number[][][]): Promise<Point[]> => {
    const response = await api.post('/points/within-polygon', polygonCoordinates);
    return response.data.data;
  }
}; 