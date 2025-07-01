import React from 'react';
import { UseMapFeaturesReturn } from '../hooks/useMapFeatures';
import { Point, Line, Polygon } from '../types';
import { calculateLineLength, calculatePolygonArea, formatDistance, formatArea } from '../utils/geometryUtils';

interface SidebarProps {
  mapFeatures: UseMapFeaturesReturn;
}

const Sidebar: React.FC<SidebarProps> = ({ mapFeatures }) => {
  const {
    points,
    lines,
    polygons,
    selectedFeature,
    loading,
    error,
    selectFeature,
    deleteSelectedFeature,
    refreshAll
  } = mapFeatures;

  const handleFeatureClick = (type: 'point' | 'line' | 'polygon', data: Point | Line | Polygon) => {
    selectFeature({ type, data });
  };

  const handleDelete = async () => {
    if (selectedFeature && confirm(`Are you sure you want to delete this ${selectedFeature.type}?`)) {
      const success = await deleteSelectedFeature();
      if (!success) {
        alert('Failed to delete feature. Please try again.');
      }
    }
  };

  const renderFeatureItem = (
    type: 'point' | 'line' | 'polygon',
    feature: Point | Line | Polygon,
    additionalInfo: string
  ) => {
    const isSelected = selectedFeature?.data.id === feature.id && selectedFeature?.type === type;
    
    return (
      <div
        key={`${type}-${feature.id}`}
        className={`feature-item ${isSelected ? 'selected' : ''}`}
        onClick={() => handleFeatureClick(type, feature)}
      >
        <div className="feature-name">{feature.name}</div>
        <div className="feature-details">{additionalInfo}</div>
      </div>
    );
  };

  const renderSelectedFeatureDetails = () => {
    if (!selectedFeature) return null;

    const { type, data } = selectedFeature;

    return (
      <div className="selected-feature">
        <h4>Selected {type.charAt(0).toUpperCase() + type.slice(1)}</h4>
        
        <div className="detail-row">
          <span className="detail-label">ID:</span>
          <span className="detail-value">{data.id}</span>
        </div>
        
        <div className="detail-row">
          <span className="detail-label">Name:</span>
          <span className="detail-value">{data.name}</span>
        </div>

        {type === 'point' && (
          <>
            <div className="detail-row">
              <span className="detail-label">Latitude:</span>
              <span className="detail-value">{(data as Point).latitude.toFixed(6)}</span>
            </div>
            <div className="detail-row">
              <span className="detail-label">Longitude:</span>
              <span className="detail-value">{(data as Point).longitude.toFixed(6)}</span>
            </div>
            {(data as Point).description && (
              <div className="detail-row">
                <span className="detail-label">Description:</span>
                <span className="detail-value">{(data as Point).description}</span>
              </div>
            )}
          </>
        )}

        {type === 'line' && (
          <>
            <div className="detail-row">
              <span className="detail-label">Length:</span>
              <span className="detail-value">
                {formatDistance(calculateLineLength((data as Line).coordinates))}
              </span>
            </div>
            <div className="detail-row">
              <span className="detail-label">Points:</span>
              <span className="detail-value">{(data as Line).coordinates.length}</span>
            </div>
            <div className="detail-row">
              <span className="detail-label">Coordinates:</span>
              <span className="detail-value">
                {(data as Line).coordinates.map((coord, idx) => 
                  `(${coord[1].toFixed(4)}, ${coord[0].toFixed(4)})`
                ).join(', ')}
              </span>
            </div>
          </>
        )}

        {type === 'polygon' && (
          <>
            <div className="detail-row">
              <span className="detail-label">Area:</span>
              <span className="detail-value">
                {formatArea(calculatePolygonArea((data as Polygon).coordinates))}
              </span>
            </div>
            <div className="detail-row">
              <span className="detail-label">Vertices:</span>
              <span className="detail-value">{(data as Polygon).coordinates[0]?.length || 0}</span>
            </div>
            <div className="detail-row">
              <span className="detail-label">Coordinates:</span>
              <span className="detail-value">
                {(data as Polygon).coordinates[0]?.map((coord, idx) => 
                  `(${coord[1].toFixed(4)}, ${coord[0].toFixed(4)})`
                ).join(', ') || 'No coordinates'}
              </span>
            </div>
          </>
        )}

        <div className="detail-row">
          <span className="detail-label">Created:</span>
          <span className="detail-value">
            {new Date(data.createdAt).toLocaleDateString()}
          </span>
        </div>

        {data.updatedAt && (
          <div className="detail-row">
            <span className="detail-label">Updated:</span>
            <span className="detail-value">
              {new Date(data.updatedAt).toLocaleDateString()}
            </span>
          </div>
        )}

        <button className="delete-button" onClick={handleDelete}>
          Delete {type}
        </button>
      </div>
    );
  };

  return (
    <div className="sidebar">
      <div className="sidebar-header">
        <h2>Map Features</h2>
        <button 
          onClick={refreshAll} 
          disabled={loading}
          style={{
            background: 'transparent',
            border: '1px solid white',
            color: 'white',
            padding: '0.25rem 0.5rem',
            borderRadius: '4px',
            cursor: 'pointer',
            fontSize: '0.75rem'
          }}
        >
          {loading ? 'Loading...' : 'Refresh'}
        </button>
      </div>

      <div className="sidebar-content">
        {error && <div className="error">{error}</div>}

        <div className="feature-list">
          <h3>Points ({points.length})</h3>
          {points.map(point => 
            renderFeatureItem(
              'point',
              point,
              `${point.latitude.toFixed(4)}, ${point.longitude.toFixed(4)}`
            )
          )}
        </div>

        <div className="feature-list">
          <h3>Lines ({lines.length})</h3>
          {lines.map(line => 
            renderFeatureItem(
              'line',
              line,
              `${formatDistance(calculateLineLength(line.coordinates))} - ${line.coordinates.length} points`
            )
          )}
        </div>

        <div className="feature-list">
          <h3>Polygons ({polygons.length})</h3>
          {polygons.map(polygon => 
            renderFeatureItem(
              'polygon',
              polygon,
              `${formatArea(calculatePolygonArea(polygon.coordinates))} - ${polygon.coordinates[0]?.length || 0} vertices`
            )
          )}
        </div>

        {renderSelectedFeatureDetails()}
      </div>
    </div>
  );
};

export default Sidebar; 