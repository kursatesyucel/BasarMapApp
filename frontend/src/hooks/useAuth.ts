import { useState, useEffect, useCallback } from 'react';
import { User, UserRole, LoginRequest, RegisterRequest } from '../types';
import { authService } from '../services/authService';

export interface UseAuthReturn {
  token: string | null;
  user: User | null;
  isAuthenticated: boolean;
  loading: boolean;
  error: string | null;
  login: (credentials: LoginRequest) => Promise<boolean>;
  register: (credentials: RegisterRequest) => Promise<boolean>;
  logout: () => void;
  clearError: () => void;
  hasRole: (roles: UserRole[]) => boolean;
}

export function useAuth(): UseAuthReturn {
  const [token, setToken] = useState<string | null>(authService.getToken());
  const [user, setUser] = useState<User | null>(authService.getUser());
  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(authService.isAuthenticated());
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Initialize auth state from localStorage
  useEffect(() => {
    const storedToken = authService.getToken();
    const storedUser = authService.getUser();
    
    if (storedToken && storedUser) {
      setToken(storedToken);
      setUser(storedUser as User);
      setIsAuthenticated(true);
    }
  }, []);

  const login = useCallback(async (credentials: LoginRequest): Promise<boolean> => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await authService.login(credentials);
      
      // Save to localStorage
      authService.saveAuth(response.token, response.username, response.role);
      
      // Update state
      setToken(response.token);
      setUser({ username: response.username, role: response.role as UserRole });
      setIsAuthenticated(true);
      
      return true;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Login failed. Please check your credentials.';
      setError(errorMessage);
      return false;
    } finally {
      setLoading(false);
    }
  }, []);

  const register = useCallback(async (credentials: RegisterRequest): Promise<boolean> => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await authService.register(credentials);
      
      // Save to localStorage
      authService.saveAuth(response.token, response.username, response.role);
      
      // Update state
      setToken(response.token);
      setUser({ username: response.username, role: response.role as UserRole });
      setIsAuthenticated(true);
      
      return true;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Registration failed. Please try again.';
      setError(errorMessage);
      return false;
    } finally {
      setLoading(false);
    }
  }, []);

  const logout = useCallback(() => {
    authService.clearAuth();
    setToken(null);
    setUser(null);
    setIsAuthenticated(false);
    setError(null);
  }, []);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  const hasRole = useCallback((roles: UserRole[]): boolean => {
    return user ? roles.includes(user.role) : false;
  }, [user]);

  return {
    token,
    user,
    isAuthenticated,
    loading,
    error,
    login,
    register,
    logout,
    clearError,
    hasRole,
  };
}
