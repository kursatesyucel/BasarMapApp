import React, { useEffect, useRef } from 'react';
import L from 'leaflet';
import 'leaflet-draw';
import { UseMapFeaturesReturn } from '../hooks/useMapFeatures';
import { pointService } from '../services/pointService';
import { lineService } from '../services/lineService';
import { polygonService } from '../services/polygonService';
import { calculateLineLength, calculatePolygonArea, formatDistance, formatArea } from '../utils/geometryUtils';

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
        featureGroup: new L.FeatureGroup([pointsLayer, linesLayer, polygonsLayer]),
        remove: true
      }
    });

    map.addControl(drawControl);

    // Handle draw events
    map.on(L.Draw.Event.CREATED, async (event: any) => {
      const { layer, layerType } = event;
      const name = prompt(`Enter name for this ${layerType}:`);
      if (!name) return;

      try {
        if (layerType === 'marker') {
          const latlng = layer.getLatLng();
          const point = await pointService.create({
            name,
            description: '',
            latitude: latlng.lat,
            longitude: latlng.lng
          });
          if (point) {
            mapFeatures.refreshPoints();
          }
        } else if (layerType === 'polyline') {
          const coordinates = layer.getLatLngs().map((latlng: L.LatLng) => [latlng.lng, latlng.lat]);
          const line = await lineService.create({
            name,
            coordinates
          });
          if (line) {
            mapFeatures.refreshLines();
          }
        } else if (layerType === 'polygon') {
          const coordinates = [layer.getLatLngs()[0].map((latlng: L.LatLng) => [latlng.lng, latlng.lat])];
          const polygon = await polygonService.create({
            name,
            coordinates
          });
          if (polygon) {
            mapFeatures.refreshPolygons();
          }
        }
      } catch (error) {
        console.error('Error creating feature:', error);
        alert('Error creating feature. Please try again.');
      }
    });

    mapInstanceRef.current = map;

    return () => {
      if (mapInstanceRef.current) {
        mapInstanceRef.current.remove();
        mapInstanceRef.current = null;
      }
    };
  }, []);

  // Update points layer
  useEffect(() => {
    if (!layersRef.current) return;

    layersRef.current.points.clearLayers();

    mapFeatures.points.forEach(point => {
      const marker = L.marker([point.latitude, point.longitude]);
      
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
    });
  }, [mapFeatures.points, mapFeatures.selectFeature]);

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

  return <div ref={mapRef} style={{ height: '100%', width: '100%' }} />;
};

export default MapView; 