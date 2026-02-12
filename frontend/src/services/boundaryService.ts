import { api } from './api';

// ============= Types =============

export interface ProvinceDto {
  id: number;
  geometry: any; // GeoJSON object
}

export interface DistrictDto {
  id: number;
  name: string;
  geometry: any; // GeoJSON object
}

export interface SettlementDto {
  id: number;
  name: string;
  category: string; // BAŞKENT, İL, İLÇE
  geometry: any; // GeoJSON object
  longitude: number;
  latitude: number;
}

// ============= API Service =============

export const boundaryService = {
  // ============= Province Boundaries =============
  
  async getAllProvinces(): Promise<ProvinceDto[]> {
    try {
      const response = await api.get('/boundaries/provinces');
      return response.data.data || [];
    } catch (error) {
      console.error('Error fetching provinces:', error);
      throw error;
    }
  },

  async getProvinceById(id: number): Promise<ProvinceDto | null> {
    try {
      const response = await api.get(`/boundaries/provinces/${id}`);
      return response.data.data || null;
    } catch (error) {
      console.error(`Error fetching province ${id}:`, error);
      return null;
    }
  },

  // ============= District Boundaries =============
  
  async getAllDistricts(): Promise<DistrictDto[]> {
    try {
      const response = await api.get('/boundaries/districts');
      return response.data.data || [];
    } catch (error) {
      console.error('Error fetching districts:', error);
      throw error;
    }
  },

  async getDistrictById(id: number): Promise<DistrictDto | null> {
    try {
      const response = await api.get(`/boundaries/districts/${id}`);
      return response.data.data || null;
    } catch (error) {
      console.error(`Error fetching district ${id}:`, error);
      return null;
    }
  },

  async searchDistrictsByName(query: string): Promise<DistrictDto[]> {
    try {
      const response = await api.get(`/boundaries/districts/search?query=${encodeURIComponent(query)}`);
      return response.data.data || [];
    } catch (error) {
      console.error('Error searching districts:', error);
      return [];
    }
  },

  // ============= Settlement Centers =============
  
  async getAllSettlements(): Promise<SettlementDto[]> {
    try {
      const response = await api.get('/boundaries/settlements');
      return response.data.data || [];
    } catch (error) {
      console.error('Error fetching settlements:', error);
      throw error;
    }
  },

  async getSettlementsByCategory(category: 'BAŞKENT' | 'İL' | 'İLÇE'): Promise<SettlementDto[]> {
    try {
      const response = await api.get(`/boundaries/settlements?category=${category}`);
      return response.data.data || [];
    } catch (error) {
      console.error(`Error fetching settlements by category ${category}:`, error);
      return [];
    }
  },

  async getSettlementsInBoundingBox(
    minLon: number, 
    minLat: number, 
    maxLon: number, 
    maxLat: number
  ): Promise<SettlementDto[]> {
    try {
      const bbox = `${minLon},${minLat},${maxLon},${maxLat}`;
      const response = await api.get(`/boundaries/settlements?bbox=${bbox}`);
      return response.data.data || [];
    } catch (error) {
      console.error('Error fetching settlements in bounding box:', error);
      return [];
    }
  },

  async getSettlementById(id: number): Promise<SettlementDto | null> {
    try {
      const response = await api.get(`/boundaries/settlements/${id}`);
      return response.data.data || null;
    } catch (error) {
      console.error(`Error fetching settlement ${id}:`, error);
      return null;
    }
  },

  // ============= Helper Methods =============
  
  /**
   * İl ve Başkent merkezlerini getirir (navigasyon için)
   */
  async getCityAndCapitalCenters(): Promise<SettlementDto[]> {
    try {
      const [capitals, cities] = await Promise.all([
        this.getSettlementsByCategory('BAŞKENT'),
        this.getSettlementsByCategory('İL')
      ]);
      return [...capitals, ...cities].sort((a, b) => a.name.localeCompare(b.name, 'tr'));
    } catch (error) {
      console.error('Error fetching city centers:', error);
      return [];
    }
  }
};
