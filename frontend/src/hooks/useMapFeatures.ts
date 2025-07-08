import { useState, useEffect, useCallback } from 'react';
import { Point, Line, Polygon, Camera, SelectedFeature } from '../types';
import { pointService } from '../services/pointService';
import { lineService } from '../services/lineService';
import { polygonService } from '../services/polygonService';
import { cameraService } from '../services/cameraService';

export interface UseMapFeaturesReturn {
  points: Point[];
  lines: Line[];
  polygons: Polygon[];
  cameras: Camera[];
  selectedFeature: SelectedFeature | null;
  loading: boolean;
  error: string | null;
  refreshAll: () => Promise<void>;
  refreshPoints: () => Promise<void>;
  refreshLines: () => Promise<void>;
  refreshPolygons: () => Promise<void>;
  refreshCameras: () => Promise<void>;
  selectFeature: (feature: SelectedFeature | null) => void;
  deleteSelectedFeature: () => Promise<boolean>;
}

export function useMapFeatures(): UseMapFeaturesReturn {
  const [points, setPoints] = useState<Point[]>([]);
  const [lines, setLines] = useState<Line[]>([]);
  const [polygons, setPolygons] = useState<Polygon[]>([]);
  const [cameras, setCameras] = useState<Camera[]>([]);
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

  const refreshCameras = useCallback(async () => {
    try {
      setError(null);
      const data = await cameraService.getAll();
      setCameras(data);
    } catch (err) {
      setError('Failed to load cameras');
      console.error('Error loading cameras:', err);
    }
  }, []);

  const refreshAll = useCallback(async () => {
    setLoading(true);
    try {
      await Promise.all([refreshPoints(), refreshLines(), refreshPolygons(), refreshCameras()]);
    } finally {
      setLoading(false);
    }
  }, [refreshPoints, refreshLines, refreshPolygons, refreshCameras]);

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
          await pointService.delete(id);
          await refreshPoints();
          success = true;
          break;
        case 'line':
          await lineService.delete(id);
          await refreshLines();
          success = true;
          break;
        case 'polygon':
          await polygonService.delete(id);
          await refreshPolygons();
          success = true;
          break;
        case 'camera':
          await cameraService.delete(id);
          await refreshCameras();
          success = true;
          break;
        default:
          return false;
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
  }, [selectedFeature, refreshPoints, refreshLines, refreshPolygons, refreshCameras]);

  // Load all features on mount
  useEffect(() => {
    refreshAll();
  }, [refreshAll]);

  return {
    points,
    lines,
    polygons,
    cameras,
    selectedFeature,
    loading,
    error,
    refreshAll,
    refreshPoints,
    refreshLines,
    refreshPolygons,
    refreshCameras,
    selectFeature,
    deleteSelectedFeature,
  };
} 