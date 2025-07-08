import { api } from './api';
import { Camera, CreateCameraDto, UpdateCameraDto } from '../types';

export const cameraService = {
  async getAll(): Promise<Camera[]> {
    const response = await api.get('/cameras');
    return response.data.data;
  },

  async getActive(): Promise<Camera[]> {
    const response = await api.get('/cameras/active');
    return response.data.data;
  },

  async getById(id: number): Promise<Camera | null> {
    try {
      const response = await api.get(`/cameras/${id}`);
      return response.data.data;
    } catch (error: any) {
      if (error.response?.status === 404) {
        return null;
      }
      throw error;
    }
  },

  async getByVideoFileName(videoFileName: string): Promise<Camera | null> {
    try {
      const response = await api.get(`/cameras/by-video/${videoFileName}`);
      return response.data.data;
    } catch (error: any) {
      if (error.response?.status === 404) {
        return null;
      }
      throw error;
    }
  },

  async create(camera: CreateCameraDto): Promise<Camera> {
    const response = await api.post('/cameras', camera);
    return response.data.data;
  },

  async update(id: number, camera: UpdateCameraDto): Promise<Camera> {
    const response = await api.put(`/cameras/${id}`, camera);
    return response.data.data;
  },

  async delete(id: number): Promise<void> {
    await api.delete(`/cameras/${id}`);
  },

  getVideoStreamUrl(videoFileName: string): string {
    return `/api/cameras/stream/${videoFileName}`;
  },

  async getCamerasWithinPolygon(polygonCoordinates: number[][][]): Promise<Camera[]> {
    const response = await api.post('/cameras/within-polygon', polygonCoordinates);
    return response.data.data;
  }
}; 