import { useState, useEffect, useCallback } from 'react';
import { Point, Line, Polygon, SelectedFeature } from '../types';
import { pointService } from '../services/pointService';
import { lineService } from '../services/lineService';
import { polygonService } from '../services/polygonService';

export interface UseMapFeaturesReturn {
  points: Point[];
  lines: Line[];
  polygons: Polygon[];
  selectedFeature: SelectedFeature | null;
  loading: boolean;
  error: string | null;
  refreshAll: () => Promise<void>;
  refreshPoints: () => Promise<void>;
  refreshLines: () => Promise<void>;
  refreshPolygons: () => Promise<void>;
  selectFeature: (feature: SelectedFeature | null) => void;
  deleteSelectedFeature: () => Promise<boolean>;
}

export function useMapFeatures(): UseMapFeaturesReturn {
  const [points, setPoints] = useState<Point[]>([]);
  const [lines, setLines] = useState<Line[]>([]);
  const [polygons, setPolygons] = useState<Polygon[]>([]);
  const [selectedFeature, setSelectedFeature] = useState<SelectedFeature | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const refreshPoints = useCallback(async () => {
    try {
      setError(null);
      const data = await pointService.getAll();
      setPoints(data);
    } catch (err) {
      setError('Failed to load points');
      console.error('Error loading points:', err);
    }
  }, []);

  const refreshLines = useCallback(async () => {
    try {
      setError(null);
      const data = await lineService.getAll();
      setLines(data);
    } catch (err) {
      setError('Failed to load lines');
      console.error('Error loading lines:', err);
    }
  }, []);

  const refreshPolygons = useCallback(async () => {
    try {
      setError(null);
      const data = await polygonService.getAll();
      setPolygons(data);
    } catch (err) {
      setError('Failed to load polygons');
      console.error('Error loading polygons:', err);
    }
  }, []);

  const refreshAll = useCallback(async () => {
    setLoading(true);
    try {
      await Promise.all([refreshPoints(), refreshLines(), refreshPolygons()]);
    } finally {
      setLoading(false);
    }
  }, [refreshPoints, refreshLines, refreshPolygons]);

  const selectFeature = useCallback((feature: SelectedFeature | null) => {
    setSelectedFeature(feature);
  }, []);

  const deleteSelectedFeature = useCallback(async (): Promise<boolean> => {
    if (!selectedFeature) return false;

    try {
      let success = false;
      const id = selectedFeature.data.id;

      switch (selectedFeature.type) {
        case 'point':
          success = await pointService.delete(id);
          if (success) await refreshPoints();
          break;
        case 'line':
          success = await lineService.delete(id);
          if (success) await refreshLines();
          break;
        case 'polygon':
          success = await polygonService.delete(id);
          if (success) await refreshPolygons();
          break;
      }

      if (success) {
        setSelectedFeature(null);
      }

      return success;
    } catch (err) {
      setError('Failed to delete feature');
      console.error('Error deleting feature:', err);
      return false;
    }
  }, [selectedFeature, refreshPoints, refreshLines, refreshPolygons]);

  // Load all features on mount
  useEffect(() => {
    refreshAll();
  }, [refreshAll]);

  return {
    points,
    lines,
    polygons,
    selectedFeature,
    loading,
    error,
    refreshAll,
    refreshPoints,
    refreshLines,
    refreshPolygons,
    selectFeature,
    deleteSelectedFeature,
  };
} 