import React, { useEffect, useRef, useState } from 'react';
import L from 'leaflet';
import 'leaflet-draw';
import { UseMapFeaturesReturn } from '../hooks/useMapFeatures';
import { pointService } from '../services/pointService';
import { lineService } from '../services/lineService';
import { polygonService } from '../services/polygonService';
import { cameraService } from '../services/cameraService';
import { calculateLineLength, calculatePolygonArea, formatDistance, formatArea } from '../utils/geometryUtils';
import FeatureForm from './FeatureForm';
import FeaturesWithinPolygonModal from './FeaturesWithinPolygonModal';
import CameraPopup from './CameraPopup';
import { CreatePointDto, CreateLineDto, CreatePolygonDto, Point, Camera } from '../types';

// Fix for default markers
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
});

// Camera icon for markers
const cameraIcon = L.divIcon({
  html: `<div style="
    background-color: #1976d2;
    border: 2px solid white;
    border-radius: 50%;
    width: 32px;
    height: 32px;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 2px 5px rgba(0,0,0,0.3);
    font-size: 16px;
    color: white;
  ">📹</div>`,
  className: 'camera-marker',
  iconSize: [32, 32],
  iconAnchor: [16, 16],
  popupAnchor: [0, -16]
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
    cameras: L.LayerGroup;
  } | null>(null);
  const editableLayersRef = useRef<L.FeatureGroup | null>(null);

  // Form state
  const [showForm, setShowForm] = useState(false);
  const [formType, setFormType] = useState<'point' | 'line' | 'polygon' | 'camera'>('point');
  const [pendingGeometry, setPendingGeometry] = useState<any>(null);

  // Features within polygon modal state
  const [showFeaturesModal, setShowFeaturesModal] = useState(false);
  const [featuresWithinPolygon, setFeaturesWithinPolygon] = useState<{points: Point[], cameras: Camera[]}>({points: [], cameras: []});
  const [currentPolygonName, setCurrentPolygonName] = useState<string>('');

  // Edit confirmation modal state
  const [showEditConfirmationModal, setShowEditConfirmationModal] = useState(false);
  const [pendingEditData, setPendingEditData] = useState<any>(null);
  const [featuresAffectedByEdit, setFeaturesAffectedByEdit] = useState<{points: Point[], cameras: Camera[]}>({points: [], cameras: []});

  // Camera state
  const { cameras } = mapFeatures;
  const [selectedCamera, setSelectedCamera] = useState<Camera | null>(null);
  const [showCameraPopup, setShowCameraPopup] = useState(false);

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
    const camerasLayer = L.layerGroup().addTo(map);

    layersRef.current = {
      points: pointsLayer,
      lines: linesLayer,
      polygons: polygonsLayer,
      cameras: camerasLayer
    };

    // Create editable feature group
    const editableLayers = new L.FeatureGroup();
    map.addLayer(editableLayers);
    editableLayersRef.current = editableLayers;

    // Initialize draw controls
    const drawControl = new L.Control.Draw({
      position: 'topright',
      draw: {
        marker: {
          icon: L.Icon.Default.prototype // Normal point marker
        },
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

    // Add custom camera drawing control
    const CameraControl = L.Control.extend({
      options: {
        position: 'topright'
      },
      
      onAdd: function(map: L.Map) {
        const container = L.DomUtil.create('div', 'leaflet-bar leaflet-control');
        const button = L.DomUtil.create('a', 'leaflet-control-camera', container);
        button.href = '#';
        button.title = 'Add Camera Point';
        button.innerHTML = '📹';
        button.style.fontSize = '16px';
        button.style.lineHeight = '26px';
        button.style.textAlign = 'center';
        button.style.textDecoration = 'none';
        button.style.color = '#333';
        button.style.backgroundColor = 'white';
        button.style.width = '26px';
        button.style.height = '26px';
        button.style.display = 'block';
        
        let drawingMode = false;
        
        L.DomEvent.on(button, 'click', function(e) {
          L.DomEvent.stopPropagation(e);
          L.DomEvent.preventDefault(e);
          
          if (!drawingMode) {
            // Enable camera drawing mode
            drawingMode = true;
            button.style.backgroundColor = '#007acc';
            button.style.color = 'white';
            map.getContainer().style.cursor = 'crosshair';
            
            // Add one-time click event
            const onMapClick = function(e: L.LeafletMouseEvent) {
              // Create camera marker
              const marker = L.marker(e.latlng, { icon: cameraIcon });
              
              // Reset drawing mode
              drawingMode = false;
              button.style.backgroundColor = 'white';
              button.style.color = '#333';
              map.getContainer().style.cursor = '';
              map.off('click', onMapClick);
              
              // Trigger form with camera type
              setPendingGeometry({ layer: marker, layerType: 'camera' });
              setFormType('camera');
              setShowForm(true);
            };
            
            map.on('click', onMapClick);
          } else {
            // Disable camera drawing mode
            drawingMode = false;
            button.style.backgroundColor = 'white';
            button.style.color = '#333';
            map.getContainer().style.cursor = '';
            map.off('click');
          }
        });
        
        return container;
      }
    });

    const cameraControl = new CameraControl();
    map.addControl(cameraControl);

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
                         layerType === 'polygon' ? 'polygon' :
                         layerType === 'camera' ? 'camera' : layerType;
      console.log('Mapping layerType:', layerType, '→ formType:', mappedFormType);
      setFormType(mappedFormType as 'point' | 'line' | 'polygon' | 'camera');
      console.log('Setting showForm to true');
      setShowForm(true);
    });

    // Handle edit events with spatial query for polygons
    map.on(L.Draw.Event.EDITED, async (event: any) => {
      const layers = event.layers;
      let updatePromises: Promise<any>[] = [];
      
      layers.eachLayer(async (layer: any) => {
        try {
          if (layer._pointId) {
            const latlng = layer.getLatLng();
            
            // Try to find in local state first
            let point = mapFeatures.points.find(p => p.id === layer._pointId || p.id == layer._pointId || String(p.id) === String(layer._pointId));
            
            // If not found in local state, fetch from API
            if (!point) {
              try {
                const fetchedPoint = await pointService.getById(layer._pointId);
                point = fetchedPoint || undefined;
              } catch (error) {
                console.error('Failed to fetch point from API:', error);
              }
            }
              
            if (point) {
              const updateData = {
                name: point.name,
                description: point.description,
                latitude: latlng.lat,
                longitude: latlng.lng
              };
              
              const updatePromise = pointService.update(layer._pointId, updateData);
              updatePromises.push(updatePromise);
            }
          } else if (layer._lineId) {
            const coordinates = layer.getLatLngs().map((latlng: L.LatLng) => [latlng.lng, latlng.lat]);
            
            // Try to find in local state first
            let line = mapFeatures.lines.find(l => l.id === layer._lineId || l.id == layer._lineId || String(l.id) === String(layer._lineId));
            
            // If not found in local state, fetch from API
            if (!line) {
              try {
                const fetchedLine = await lineService.getById(layer._lineId);
                line = fetchedLine || undefined;
              } catch (error) {
                console.error('Failed to fetch line from API:', error);
              }
            }
              
            if (line) {
              const updateData = {
                name: line.name,
                description: line.description,
                coordinates
              };
              
              const updatePromise = lineService.update(layer._lineId, updateData);
              updatePromises.push(updatePromise);
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
            
            // Try to find in local state first
            let polygon = mapFeatures.polygons.find(p => p.id === layer._polygonId || p.id == layer._polygonId || String(p.id) === String(layer._polygonId));
            
            // If not found in local state, fetch from API
            if (!polygon) {
              try {
                const fetchedPolygon = await polygonService.getById(layer._polygonId);
                polygon = fetchedPolygon || undefined;
              } catch (error) {
                console.error('Failed to fetch polygon from API:', error);
              }
            }
              
            if (polygon) {
              // Check for points and cameras within the edited polygon BEFORE saving
              try {
                const [pointsWithin, camerasWithin] = await Promise.all([
                  pointService.getPointsWithinPolygon(coordinates),
                  cameraService.getCamerasWithinPolygon(coordinates)
                ]);
                
                if (pointsWithin.length > 0 || camerasWithin.length > 0) {
                  // Show confirmation modal with affected features
                  setPendingEditData({
                    polygonId: layer._polygonId,
                    polygon: polygon,
                    coordinates: coordinates
                  });
                  setFeaturesAffectedByEdit({points: pointsWithin, cameras: camerasWithin});
                  setShowEditConfirmationModal(true);
                  return; // Don't save yet, wait for user confirmation
                }
              } catch (error) {
                console.error('Error checking features within edited polygon:', error);
              }
              
              // No points affected, proceed with normal update
              const updateData = {
                name: polygon.name,
                description: polygon.description,
                coordinates
              };
              
              const updatePromise = polygonService.update(layer._polygonId, updateData);
              updatePromises.push(updatePromise);
            }
          } else if (layer._cameraId) {
            const latlng = layer.getLatLng();
            
            // Try to find in local state first
            let camera = cameras.find(c => c.id === layer._cameraId || c.id == layer._cameraId || String(c.id) === String(layer._cameraId));
            
            // If not found in local state, fetch from API
            if (!camera) {
              try {
                const fetchedCamera = await cameraService.getById(layer._cameraId);
                camera = fetchedCamera || undefined;
              } catch (error) {
                console.error('Failed to fetch camera from API:', error);
              }
            }
              
            if (camera) {
              const updateData = {
                name: camera.name,
                description: camera.description,
                latitude: latlng.lat,
                longitude: latlng.lng,
                videoFileName: camera.videoFileName,
                isActive: camera.isActive
              };
              
              const updatePromise = cameraService.update(layer._cameraId, updateData);
              updatePromises.push(updatePromise);
            }
          }
        } catch (error) {
          console.error('Error processing layer:', error);
        }
      });

      // Wait for all updates to complete, then refresh
      if (updatePromises.length > 0) {
        try {
          await Promise.all(updatePromises);
          mapFeatures.refreshAll();
        } catch (error) {
          console.error('Error during batch update:', error);
          // Still refresh to ensure UI consistency
          mapFeatures.refreshAll();
        }
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



  // Camera click handler
  const handleCameraClick = (camera: Camera) => {
    setSelectedCamera(camera);
    setShowCameraPopup(true);
  };

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
        const lineData = data as CreateLineDto;
        console.log('Creating line with data:', { ...lineData, coordinates });
        const line = await lineService.create({
          name: lineData.name,
          description: lineData.description,
          coordinates
        });
        console.log('Line creation result:', line);
        if (line) {
          mapFeatures.refreshLines();
        }
      } else if (layerType === 'camera') {
        const latlng = layer.getLatLng();
        console.log('Creating camera with data:', { ...data, latitude: latlng.lat, longitude: latlng.lng });
        const camera = await cameraService.create({
          name: data.name,
          description: data.description,
          latitude: latlng.lat,
          longitude: latlng.lng,
          videoFileName: data.videoFileName,
          isActive: data.isActive
        });
        console.log('Camera creation result:', camera);
        if (camera) {
          mapFeatures.refreshCameras();
        }
      } else if (layerType === 'polygon') {
        const outerRing = layer.getLatLngs()[0].map((latlng: L.LatLng) => [latlng.lng, latlng.lat]);
        
        // Ensure the polygon is closed
        if (outerRing.length > 0 && 
            (outerRing[0][0] !== outerRing[outerRing.length - 1][0] || 
             outerRing[0][1] !== outerRing[outerRing.length - 1][1])) {
          outerRing.push([outerRing[0][0], outerRing[0][1]]);
        }
        const coordinates = [outerRing];
        const polygonData = data as CreatePolygonDto;
        console.log('Creating polygon with data:', { ...polygonData, coordinates });
        const polygon = await polygonService.createWithIntersectionHandling({
          name: polygonData.name,
          description: polygonData.description,
          coordinates
        });
        console.log('Polygon creation result:', polygon);
        if (polygon) {
          mapFeatures.refreshPolygons();
          
          // After polygon creation, check for points and cameras within and show modal
          try {
            const [pointsWithin, camerasWithin] = await Promise.all([
              pointService.getPointsWithinPolygon(coordinates),
              cameraService.getCamerasWithinPolygon(coordinates)
            ]);
            
            if (pointsWithin.length > 0 || camerasWithin.length > 0) {
              setFeaturesWithinPolygon({points: pointsWithin, cameras: camerasWithin});
              setCurrentPolygonName(polygonData.name);
              setShowFeaturesModal(true);
            }
          } catch (error) {
            console.error('Error checking features within polygon:', error);
          }
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

  const handleFeaturesModalClose = () => {
    setShowFeaturesModal(false);
    setFeaturesWithinPolygon({points: [], cameras: []});
    setCurrentPolygonName('');
  };

  const handleEditConfirmation = async (proceed: boolean) => {
    if (!pendingEditData) return;
    
    if (proceed) {
      // User confirmed, proceed with the edit
      try {
        const updateData = {
          name: pendingEditData.polygon.name,
          description: pendingEditData.polygon.description,
          coordinates: pendingEditData.coordinates
        };
        
        await polygonService.update(pendingEditData.polygonId, updateData);
        mapFeatures.refreshPolygons();
      } catch (error) {
        console.error('Error updating polygon:', error);
      }
    } else {
      // User cancelled, refresh to revert visual changes
      mapFeatures.refreshPolygons();
    }
    
    // Close modal and clear state
    setShowEditConfirmationModal(false);
    setPendingEditData(null);
    setFeaturesAffectedByEdit({points: [], cameras: []});
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

    // Add cameras to editable layers  
    cameras.forEach(camera => {
      const marker = L.marker([camera.latitude, camera.longitude], { icon: cameraIcon });
      (marker as any)._cameraId = camera.id;
      (marker as any)._featureType = 'camera';
      
      // Add click handler for camera popup
      marker.on('click', () => {
        handleCameraClick(camera);
      });
      
      editableLayersRef.current!.addLayer(marker);
    });
  }, [mapFeatures.points, mapFeatures.lines, mapFeatures.polygons, cameras, mapFeatures.selectFeature]);

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

  // Update cameras layer
  useEffect(() => {
    if (!layersRef.current) return;

    layersRef.current.cameras.clearLayers();

    cameras.forEach(camera => {
      const marker = L.marker([camera.latitude, camera.longitude], { icon: cameraIcon });
      
      const popupContent = `
        <div class="popup-content">
          <h4>${camera.name}</h4>
          <div class="popup-details">
            <div>Status: ${camera.isActive ? '🟢 Aktif' : '🔴 Pasif'}</div>
            <div>Lat: ${camera.latitude.toFixed(6)}</div>
            <div>Lng: ${camera.longitude.toFixed(6)}</div>
            ${camera.description ? `<div>Description: ${camera.description}</div>` : ''}
            <div><small>Tıkla: Video izle</small></div>
          </div>
        </div>
      `;
      
      marker.bindPopup(popupContent);
      marker.on('click', () => handleCameraClick(camera));
      
      layersRef.current!.cameras.addLayer(marker);
    });
  }, [cameras]);

  // Pan to selected feature
  useEffect(() => {
    if (!mapInstanceRef.current || !mapFeatures.selectedFeature) return;

    const { type, data } = mapFeatures.selectedFeature;

    if (type === 'point') {
      const point = data as any;
      mapInstanceRef.current.setView([point.latitude, point.longitude], 15);
    } else if (type === 'camera') {
      const camera = data as any;
      mapInstanceRef.current.setView([camera.latitude, camera.longitude], 15);
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
      <FeaturesWithinPolygonModal
        isOpen={showFeaturesModal}
        points={featuresWithinPolygon.points}
        cameras={featuresWithinPolygon.cameras}
        onClose={handleFeaturesModalClose}
        polygonName={currentPolygonName}
      />
      <FeaturesWithinPolygonModal
        isOpen={showEditConfirmationModal}
        points={featuresAffectedByEdit.points}
        cameras={featuresAffectedByEdit.cameras}
        onClose={() => handleEditConfirmation(false)}
        polygonName={`Edited ${pendingEditData?.polygon?.name || 'Polygon'}`}
        showEditConfirmation={true}
        onConfirmEdit={() => handleEditConfirmation(true)}
        onCancelEdit={() => handleEditConfirmation(false)}
      />
      {selectedCamera && (
        <CameraPopup
          camera={selectedCamera}
          isOpen={showCameraPopup}
          onClose={() => {
            setShowCameraPopup(false);
            setSelectedCamera(null);
          }}
        />
      )}
    </>
  );
};

export default MapView; 