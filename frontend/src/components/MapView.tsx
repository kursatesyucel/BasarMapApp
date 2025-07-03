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
      console.log('🎯 EDIT EVENT TRIGGERED!', event);
      const layers = event.layers;
      console.log('📦 Layers to update:', layers);
      
      let updatePromises: Promise<any>[] = [];
      
      layers.eachLayer(async (layer: any) => {
        console.log('🔍 Processing layer:', layer);
        console.log('Layer properties:', {
          _pointId: layer._pointId,
          _lineId: layer._lineId,
          _polygonId: layer._polygonId,
          _featureType: layer._featureType
        });

        try {
                      if (layer._pointId) {
              console.log('📍 Updating POINT with ID:', layer._pointId, 'Type:', typeof layer._pointId);
              const latlng = layer.getLatLng();
              console.log('📍 New coordinates:', { lat: latlng.lat, lng: latlng.lng });
              
                            console.log('📍 Available points in mapFeatures:', mapFeatures.points.map(p => ({ id: p.id, name: p.name, idType: typeof p.id })));
              
              // Try to find in local state first
              let point = mapFeatures.points.find(p => p.id === layer._pointId || p.id == layer._pointId || String(p.id) === String(layer._pointId));
              
              console.log('📍 Found point data in local state:', point);
              
                              // If not found in local state, fetch from API
                if (!point) {
                  console.log('📍 Point not in local state, fetching from API...');
                  try {
                    const fetchedPoint = await pointService.getById(layer._pointId);
                    point = fetchedPoint || undefined;
                    console.log('📍 Point fetched from API:', point);
                  } catch (error) {
                    console.error('📍 Failed to fetch point from API:', error);
                  }
                }
              
              if (point) {
              const updateData = {
                name: point.name,
                description: point.description,
                latitude: latlng.lat,
                longitude: latlng.lng
              };
              console.log('📍 Sending update data:', updateData);
              
              const updatePromise = pointService.update(layer._pointId, updateData)
                .then(result => {
                  console.log('✅ Point update successful:', result);
                  return result;
                })
                .catch(error => {
                  console.error('❌ Point update failed:', error);
                  throw error;
                });
              
              updatePromises.push(updatePromise);
            } else {
              console.warn('⚠️ Point not found in mapFeatures.points');
            }
                      } else if (layer._lineId) {
              console.log('📏 Updating LINE with ID:', layer._lineId, 'Type:', typeof layer._lineId);
              const coordinates = layer.getLatLngs().map((latlng: L.LatLng) => [latlng.lng, latlng.lat]);
              console.log('📏 New coordinates:', coordinates);
              
                            console.log('📏 Available lines in mapFeatures:', mapFeatures.lines.map(l => ({ id: l.id, name: l.name, idType: typeof l.id })));
              
              // Try to find in local state first
              let line = mapFeatures.lines.find(l => l.id === layer._lineId || l.id == layer._lineId || String(l.id) === String(layer._lineId));
              
              console.log('📏 Found line data in local state:', line);
              
              // If not found in local state, fetch from API
              if (!line) {
                console.log('📏 Line not in local state, fetching from API...');
                try {
                  const fetchedLine = await lineService.getById(layer._lineId);
                  line = fetchedLine || undefined;
                  console.log('📏 Line fetched from API:', line);
                } catch (error) {
                  console.error('📏 Failed to fetch line from API:', error);
                }
              }
              
              if (line) {
              const updateData = {
                name: line.name,
                description: line.description,
                coordinates
              };
              console.log('📏 Sending update data:', updateData);
              
              const updatePromise = lineService.update(layer._lineId, updateData)
                .then(result => {
                  console.log('✅ Line update successful:', result);
                  return result;
                })
                .catch(error => {
                  console.error('❌ Line update failed:', error);
                  throw error;
                });
              
              updatePromises.push(updatePromise);
            } else {
              console.warn('⚠️ Line not found in mapFeatures.lines');
            }
                      } else if (layer._polygonId) {
              console.log('🔷 Updating POLYGON with ID:', layer._polygonId, 'Type:', typeof layer._polygonId);
              const outerRing = layer.getLatLngs()[0].map((latlng: L.LatLng) => [latlng.lng, latlng.lat]);
              console.log('🔷 Raw outer ring:', outerRing);
              
              // Ensure the polygon is closed
              if (outerRing.length > 0 && 
                  (outerRing[0][0] !== outerRing[outerRing.length - 1][0] || 
                   outerRing[0][1] !== outerRing[outerRing.length - 1][1])) {
                outerRing.push([outerRing[0][0], outerRing[0][1]]);
                console.log('🔷 Polygon closed, final coordinates:', outerRing);
              }
              
              const coordinates = [outerRing];
              
                            console.log('🔷 Available polygons in mapFeatures:', mapFeatures.polygons.map(p => ({ id: p.id, name: p.name, idType: typeof p.id })));
              
              // Try to find in local state first
              let polygon = mapFeatures.polygons.find(p => p.id === layer._polygonId || p.id == layer._polygonId || String(p.id) === String(layer._polygonId));
              
              console.log('🔷 Found polygon data in local state:', polygon);
              
              // If not found in local state, fetch from API
              if (!polygon) {
                console.log('🔷 Polygon not in local state, fetching from API...');
                try {
                  const fetchedPolygon = await polygonService.getById(layer._polygonId);
                  polygon = fetchedPolygon || undefined;
                  console.log('🔷 Polygon fetched from API:', polygon);
                } catch (error) {
                  console.error('🔷 Failed to fetch polygon from API:', error);
                }
              }
              
              if (polygon) {
              const updateData = {
                name: polygon.name,
                description: polygon.description,
                coordinates
              };
              console.log('🔷 Sending update data:', updateData);
              
              const updatePromise = polygonService.update(layer._polygonId, updateData)
                .then(result => {
                  console.log('✅ Polygon update successful:', result);
                  return result;
                })
                .catch(error => {
                  console.error('❌ Polygon update failed:', error);
                  throw error;
                });
              
              updatePromises.push(updatePromise);
            } else {
              console.warn('⚠️ Polygon not found in mapFeatures.polygons');
            }
          } else {
            console.warn('⚠️ Layer has no recognized ID:', {
              _pointId: layer._pointId,
              _lineId: layer._lineId,
              _polygonId: layer._polygonId
            });
          }
        } catch (error) {
          console.error('💥 Error processing layer:', error);
        }
      });

      // Wait for all updates to complete, then refresh
      if (updatePromises.length > 0) {
        try {
          console.log('⏳ Waiting for all updates to complete...');
          await Promise.all(updatePromises);
          console.log('🔄 All updates completed, refreshing data...');
          
          // Refresh all features to get updated data
          mapFeatures.refreshAll();
          console.log('✅ Data refresh completed!');
        } catch (error) {
          console.error('💥 Error during batch update:', error);
          // Still refresh to ensure UI consistency
          mapFeatures.refreshAll();
        }
      } else {
        console.warn('⚠️ No valid layers found for update');
      }
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
      console.log('📍 Creating point marker:', { id: point.id, name: point.name, _pointId: (marker as any)._pointId });
      
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
      console.log('📏 Creating line polyline:', { id: line.id, name: line.name, _lineId: (polyline as any)._lineId });
      
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
      console.log('🔷 Creating polygon:', { id: polygon.id, name: polygon.name, _polygonId: (leafletPolygon as any)._polygonId });
      
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