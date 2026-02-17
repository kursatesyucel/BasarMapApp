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

// Auth Types
export type UserRole = 'User' | 'Manager' | 'Admin';

export interface User {
  username: string;
  role: UserRole;
}

export interface AuthState {
  token: string | null;
  user: User | null;
  isAuthenticated: boolean;
  loading: boolean;
  error: string | null;
}

export interface LoginRequest {
  loginIdentifier: string; // Username or Email
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface VerifyEmailRequest {
  email: string;
  code: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
  confirmPassword: string;
}

export interface AuthResponse {
  token: string;
  username: string;
  role: string;
}

export interface UserListItem {
  id: number;
  username: string;
  email: string;
  role: string;
  isEmailConfirmed: boolean;
  isActive: boolean;
  createdAt: string;
}

export interface UpdateUserRoleRequest {
  role: string;
}

export interface UpdateUserStatusRequest {
  isActive: boolean;
} 