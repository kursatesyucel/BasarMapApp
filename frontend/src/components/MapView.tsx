import React, { useEffect, useRef, useState } from 'react';
import L from 'leaflet';
import 'leaflet-draw';
import { UseMapFeaturesReturn } from '../hooks/useMapFeatures';
import { pointService } from '../services/pointService';
import { lineService } from '../services/lineService';
import { polygonService } from '../services/polygonService';
import { calculateLineLength, calculatePolygonArea, formatDistance, formatArea } from '../utils/geometryUtils';
import FeatureForm from './FeatureForm';
import { CreatePointDto, CreateLineDto, CreatePolygonDto } from '../types';

// Fix for default markers
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
});

interface MapViewProps {
  mapFeatures: UseMapFeaturesReturn;
}

const MapView: React.FC<MapViewProps> = ({ mapFeatures }) => {
  const mapRef = useRef<HTMLDivElement>(null);
  const mapInstanceRef = useRef<L.Map | null>(null);
  const layersRef = useRef<{
    points: L.LayerGroup;
    lines: L.LayerGroup;
    polygons: L.LayerGroup;
  } | null>(null);
  const editableLayersRef = useRef<L.FeatureGroup | null>(null);

  // Form state
  const [showForm, setShowForm] = useState(false);
  const [formType, setFormType] = useState<'point' | 'line' | 'polygon'>('point');
  const [pendingGeometry, setPendingGeometry] = useState<any>(null);

  useEffect(() => {
    if (!mapRef.current || mapInstanceRef.current) return;

    // Initialize map
    const map = L.map(mapRef.current).setView([39.0, 35.0], 6); // Turkey center

    // Add tile layer
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    // Create layer groups
    const pointsLayer = L.layerGroup().addTo(map);
    const linesLayer = L.layerGroup().addTo(map);
    const polygonsLayer = L.layerGroup().addTo(map);

    layersRef.current = {
      points: pointsLayer,
      lines: linesLayer,
      polygons: polygonsLayer
    };

    // Create editable feature group
    const editableLayers = new L.FeatureGroup();
    map.addLayer(editableLayers);
    editableLayersRef.current = editableLayers;

    // Initialize draw controls
    const drawControl = new L.Control.Draw({
      position: 'topright',
      draw: {
        marker: {},
        polyline: {},
        polygon: {},
        rectangle: false,
        circle: false,
        circlemarker: false
      },
      edit: {
        featureGroup: editableLayers,
        remove: true
      }
    });

    map.addControl(drawControl);

    // Handle draw events
    map.on(L.Draw.Event.CREATED, (event: any) => {
      console.log('Draw event created:', event);
      const { layer, layerType } = event;
      console.log('Layer type:', layerType);
      
      // Store the geometry and show form (don't add to editableLayers yet)
      setPendingGeometry({ layer, layerType });
      const mappedFormType = layerType === 'marker' ? 'point' : 
                         layerType === 'polyline' ? 'line' : 
                         layerType === 'polygon' ? 'polygon' : layerType;
      console.log('Mapping layerType:', layerType, '→ formType:', mappedFormType);
      setFormType(mappedFormType as 'point' | 'line' | 'polygon');
      console.log('Setting showForm to true');
      setShowForm(true);
    });

    // Handle edit events
    map.on(L.Draw.Event.EDITED, async (event: any) => {
      const layers = event.layers;
      layers.eachLayer(async (layer: any) => {
        try {
          if (layer._pointId) {
            const latlng = layer.getLatLng();
            const point = mapFeatures.points.find(p => p.id === layer._pointId);
            if (point) {
              await pointService.update(layer._pointId, {
                name: point.name,
                description: point.description,
                latitude: latlng.lat,
                longitude: latlng.lng
              });
              mapFeatures.refreshPoints();
            }
          } else if (layer._lineId) {
            const coordinates = layer.getLatLngs().map((latlng: L.LatLng) => [latlng.lng, latlng.lat]);
            const line = mapFeatures.lines.find(l => l.id === layer._lineId);
            if (line) {
              await lineService.update(layer._lineId, {
                name: line.name,
                description: line.description,
                coordinates
              });
              mapFeatures.refreshLines();
            }
          } else if (layer._polygonId) {
            const outerRing = layer.getLatLngs()[0].map((latlng: L.LatLng) => [latlng.lng, latlng.lat]);
            // Ensure the polygon is closed
            if (outerRing.length > 0 && 
                (outerRing[0][0] !== outerRing[outerRing.length - 1][0] || 
                 outerRing[0][1] !== outerRing[outerRing.length - 1][1])) {
              outerRing.push([outerRing[0][0], outerRing[0][1]]);
            }
            const coordinates = [outerRing];
            const polygon = mapFeatures.polygons.find(p => p.id === layer._polygonId);
            if (polygon) {
              await polygonService.update(layer._polygonId, {
                name: polygon.name,
                description: polygon.description,
                coordinates
              });
              mapFeatures.refreshPolygons();
            }
          }
        } catch (error) {
          console.error('Error updating feature:', error);
        }
      });
    });

    // Handle delete events
    map.on(L.Draw.Event.DELETED, async (event: any) => {
      const layers = event.layers;
      layers.eachLayer(async (layer: any) => {
        try {
          if (layer._pointId) {
            await pointService.delete(layer._pointId);
            mapFeatures.refreshPoints();
          } else if (layer._lineId) {
            await lineService.delete(layer._lineId);
            mapFeatures.refreshLines();
          } else if (layer._polygonId) {
            await polygonService.delete(layer._polygonId);
            mapFeatures.refreshPolygons();
          }
        } catch (error) {
          console.error('Error deleting feature:', error);
        }
      });
    });

    mapInstanceRef.current = map;

    return () => {
      if (mapInstanceRef.current) {
        mapInstanceRef.current.remove();
        mapInstanceRef.current = null;
      }
    };
  }, []);

  // Form handlers
  const handleFormSubmit = async (data: any) => {
    console.log('MapView.handleFormSubmit called with data:', data);
    
    if (!pendingGeometry) {
      console.log('No pending geometry, returning');
      return;
    }

    const { layer, layerType } = pendingGeometry;
    console.log('Pending geometry:', { layerType });

    try {
      if (layerType === 'marker') {
        const latlng = layer.getLatLng();
        console.log('Creating point with data:', { ...data, latitude: latlng.lat, longitude: latlng.lng });
        const point = await pointService.create({
          name: data.name,
          description: data.description,
          latitude: latlng.lat,
          longitude: latlng.lng
        });
        console.log('Point creation result:', point);
        if (point) {
          mapFeatures.refreshPoints();
        }
      } else if (layerType === 'polyline') {
        const coordinates = layer.getLatLngs().map((latlng: L.LatLng) => [latlng.lng, latlng.lat]);
        console.log('Creating line with data:', { name: data.name, description: data.description, coordinates });
        const line = await lineService.create({
          name: data.name,
          description: data.description,
          coordinates
        });
        console.log('Line creation result:', line);
        if (line) {
          mapFeatures.refreshLines();
        }
      } else if (layerType === 'polygon') {
        const outerRing = layer.getLatLngs()[0].map((latlng: L.LatLng) => [latlng.lng, latlng.lat]);
        // Ensure the polygon is closed (first point = last point)
        if (outerRing.length > 0 && 
            (outerRing[0][0] !== outerRing[outerRing.length - 1][0] || 
             outerRing[0][1] !== outerRing[outerRing.length - 1][1])) {
          outerRing.push([outerRing[0][0], outerRing[0][1]]);
        }
        const coordinates = [outerRing];
        const polygonData = data as CreatePolygonDto;
        console.log('Creating polygon with data:', { ...polygonData, coordinates });
        const polygon = await polygonService.create({
          name: polygonData.name,
          description: polygonData.description,
          coordinates
        });
        console.log('Polygon creation result:', polygon);
        if (polygon) {
          mapFeatures.refreshPolygons();
        }
      }
      
      handleFormCancel();
    } catch (error) {
      console.error('Error creating feature:', error);
      throw error;
    }
  };

  const handleFormCancel = () => {
    setShowForm(false);
    setPendingGeometry(null);
  };

  // Update points layer
  useEffect(() => {
    if (!layersRef.current || !editableLayersRef.current) return;

    // Clear all layers
    layersRef.current.points.clearLayers();
    editableLayersRef.current.clearLayers();

    mapFeatures.points.forEach(point => {
      const marker = L.marker([point.latitude, point.longitude]);
      (marker as any)._pointId = point.id; // Store ID for editing
      (marker as any)._featureType = 'point';
      
      const popupContent = `
        <div class="popup-content">
          <h4>${point.name}</h4>
          <div class="popup-details">
            <div>Lat: ${point.latitude.toFixed(6)}</div>
            <div>Lng: ${point.longitude.toFixed(6)}</div>
            ${point.description ? `<div>Description: ${point.description}</div>` : ''}
          </div>
        </div>
      `;
      
      marker.bindPopup(popupContent);
      marker.on('click', () => {
        mapFeatures.selectFeature({ type: 'point', data: point });
      });
      
      layersRef.current!.points.addLayer(marker);
      editableLayersRef.current!.addLayer(marker);
    });

    // Add lines to editable layers
    mapFeatures.lines.forEach(line => {
      const polyline = L.polyline(
        line.coordinates.map(coord => [coord[1], coord[0]]),
        { color: 'blue', weight: 3 }
      );
      (polyline as any)._lineId = line.id;
      (polyline as any)._featureType = 'line';
      
      editableLayersRef.current!.addLayer(polyline);
    });

    // Add polygons to editable layers
    mapFeatures.polygons.forEach(polygon => {
      const leafletPolygon = L.polygon(
        polygon.coordinates.map(ring => 
          ring.map(coord => [coord[1], coord[0]] as L.LatLngTuple)
        ),
        { color: 'red', weight: 2, fillOpacity: 0.3 }
      );
      (leafletPolygon as any)._polygonId = polygon.id;
      (leafletPolygon as any)._featureType = 'polygon';
      
      editableLayersRef.current!.addLayer(leafletPolygon);
    });
  }, [mapFeatures.points, mapFeatures.lines, mapFeatures.polygons, mapFeatures.selectFeature]);

  // Update lines layer
  useEffect(() => {
    if (!layersRef.current) return;

    layersRef.current.lines.clearLayers();

    mapFeatures.lines.forEach(line => {
      const polyline = L.polyline(
        line.coordinates.map(coord => [coord[1], coord[0]]),
        { color: 'blue', weight: 3 }
      );
      
      const length = calculateLineLength(line.coordinates);
      const popupContent = `
        <div class="popup-content">
          <h4>${line.name}</h4>
          <div class="popup-details">
            <div>Length: ${formatDistance(length)}</div>
            <div>Points: ${line.coordinates.length}</div>
            ${line.description ? `<div>Description: ${line.description}</div>` : ''}
          </div>
        </div>
      `;
      
      polyline.bindPopup(popupContent);
      polyline.on('click', () => {
        mapFeatures.selectFeature({ type: 'line', data: line });
      });
      
      layersRef.current!.lines.addLayer(polyline);
    });
  }, [mapFeatures.lines, mapFeatures.selectFeature]);

  // Update polygons layer
  useEffect(() => {
    if (!layersRef.current) return;

    layersRef.current.polygons.clearLayers();

    mapFeatures.polygons.forEach(polygon => {
      const leafletPolygon = L.polygon(
        polygon.coordinates.map(ring => 
          ring.map(coord => [coord[1], coord[0]] as L.LatLngTuple)
        ),
        { color: 'red', weight: 2, fillOpacity: 0.3 }
      );
      
      const area = calculatePolygonArea(polygon.coordinates);
      const popupContent = `
        <div class="popup-content">
          <h4>${polygon.name}</h4>
          <div class="popup-details">
            <div>Area: ${formatArea(area)}</div>
            <div>Vertices: ${polygon.coordinates[0]?.length || 0}</div>
            ${polygon.description ? `<div>Description: ${polygon.description}</div>` : ''}
          </div>
        </div>
      `;
      
      leafletPolygon.bindPopup(popupContent);
      leafletPolygon.on('click', () => {
        mapFeatures.selectFeature({ type: 'polygon', data: polygon });
      });
      
      layersRef.current!.polygons.addLayer(leafletPolygon);
    });
  }, [mapFeatures.polygons, mapFeatures.selectFeature]);

  // Pan to selected feature
  useEffect(() => {
    if (!mapInstanceRef.current || !mapFeatures.selectedFeature) return;

    const { type, data } = mapFeatures.selectedFeature;

    if (type === 'point') {
      const point = data as any;
      mapInstanceRef.current.setView([point.latitude, point.longitude], 15);
    } else if (type === 'line') {
      const line = data as any;
      if (line.coordinates.length > 0) {
        const bounds = L.latLngBounds(
          line.coordinates.map((coord: number[]) => [coord[1], coord[0]])
        );
        mapInstanceRef.current.fitBounds(bounds);
      }
    } else if (type === 'polygon') {
      const polygon = data as any;
      if (polygon.coordinates.length > 0 && polygon.coordinates[0].length > 0) {
        const bounds = L.latLngBounds(
          polygon.coordinates[0].map((coord: number[]) => [coord[1], coord[0]])
        );
        mapInstanceRef.current.fitBounds(bounds);
      }
    }
  }, [mapFeatures.selectedFeature]);

  return (
    <>
      <div ref={mapRef} style={{ height: '100%', width: '100%' }} />
      <FeatureForm
        isOpen={showForm}
        featureType={formType}
        onSubmit={handleFormSubmit}
        onCancel={handleFormCancel}
        title={`Create ${formType.charAt(0).toUpperCase() + formType.slice(1)}`}
      />
    </>
  );
};

export default MapView; 