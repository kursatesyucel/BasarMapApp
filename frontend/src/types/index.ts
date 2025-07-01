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
  coordinates: number[][];
  createdAt: string;
  updatedAt?: string;
}

export interface Polygon {
  id: number;
  name: string;
  coordinates: number[][][];
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
  coordinates: number[][];
}

export interface CreatePolygonDto {
  name: string;
  coordinates: number[][][];
}

export interface UpdatePointDto {
  name: string;
  description?: string;
  latitude: number;
  longitude: number;
}

export interface UpdateLineDto {
  name: string;
  coordinates: number[][];
}

export interface UpdatePolygonDto {
  name: string;
  coordinates: number[][][];
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors?: string[];
}

export type FeatureType = 'point' | 'line' | 'polygon';

export interface SelectedFeature {
  type: FeatureType;
  data: Point | Line | Polygon;
} 