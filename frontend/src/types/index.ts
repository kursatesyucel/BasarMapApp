export interface Point {
  id: number;
  name: string;
  description?: string;
  latitude: number;
  longitude: number;
  createdAt: string;
  updatedAt?: string;
}

export interface Line {
  id: number;
  name: string;
  description?: string;
  coordinates: number[][];
  createdAt: string;
  updatedAt?: string;
}

export interface Polygon {
  id: number;
  name: string;
  description?: string;
  coordinates: number[][][];
  createdAt: string;
  updatedAt?: string;
}

export interface Camera {
  id: number;
  name: string;
  description?: string;
  latitude: number;
  longitude: number;
  videoFileName: string;
  videoUrl: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreatePointDto {
  name: string;
  description?: string;
  latitude: number;
  longitude: number;
}

export interface CreateLineDto {
  name: string;
  description?: string;
  coordinates: number[][];
}

export interface CreatePolygonDto {
  name: string;
  description?: string;
  coordinates: number[][][];
}

export interface CreateCameraDto {
  name: string;
  description?: string;
  latitude: number;
  longitude: number;
  videoFileName: string;
  isActive?: boolean;
}

export interface UpdatePointDto {
  name: string;
  description?: string;
  latitude: number;
  longitude: number;
}

export interface UpdateLineDto {
  name: string;
  description?: string;
  coordinates: number[][];
}

export interface UpdatePolygonDto {
  name: string;
  description?: string;
  coordinates: number[][][];
}

export interface UpdateCameraDto {
  name: string;
  description?: string;
  latitude: number;
  longitude: number;
  videoFileName: string;
  isActive?: boolean;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors?: string[];
}

export type FeatureType = 'point' | 'line' | 'polygon' | 'camera';

export interface SelectedFeature {
  type: FeatureType;
  data: Point | Line | Polygon | Camera;
} 