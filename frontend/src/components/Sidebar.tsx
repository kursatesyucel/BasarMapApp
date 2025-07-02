import React, { useState } from 'react';
import { UseMapFeaturesReturn } from '../hooks/useMapFeatures';
import { Point, Line, Polygon, UpdatePointDto, UpdateLineDto, UpdatePolygonDto } from '../types';
import { calculateLineLength, calculatePolygonArea, formatDistance, formatArea } from '../utils/geometryUtils';
import { pointService } from '../services/pointService';
import { lineService } from '../services/lineService';
import { polygonService } from '../services/polygonService';
import EditFeatureModal from './EditFeatureModal';

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

  const [showEditModal, setShowEditModal] = useState(false);
  const [editingFeature, setEditingFeature] = useState<{
    feature: Point | Line | Polygon;
    type: 'point' | 'line' | 'polygon';
  } | null>(null);

  const handleFeatureClick = (type: 'point' | 'line' | 'polygon', data: Point | Line | Polygon) => {
    selectFeature({ type, data });
  };

  const handleEdit = () => {
    if (selectedFeature) {
      setEditingFeature({
        feature: selectedFeature.data,
        type: selectedFeature.type
      });
      setShowEditModal(true);
    }
  };

  const handleEditSubmit = async (data: UpdatePointDto | UpdateLineDto | UpdatePolygonDto): Promise<boolean> => {
    if (!editingFeature) return false;

    try {
      let success = false;
      
      if (editingFeature.type === 'point') {
        const result = await pointService.update(editingFeature.feature.id, data as UpdatePointDto);
        success = !!result;
      } else if (editingFeature.type === 'line') {
        const result = await lineService.update(editingFeature.feature.id, data as UpdateLineDto);
        success = !!result;
      } else if (editingFeature.type === 'polygon') {
        const result = await polygonService.update(editingFeature.feature.id, data as UpdatePolygonDto);
        success = !!result;
      }

      if (success) {
        // Refresh data
        refreshAll();
        // Clear selection to force re-selection with updated data
        selectFeature(null);
      }

      return success;
    } catch (error) {
      console.error('Error updating feature:', error);
      return false;
    }
  };

  const handleEditCancel = () => {
    setShowEditModal(false);
    setEditingFeature(null);
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
    const hasDescription = 'description' in feature && feature.description;
    
    return (
      <div
        key={`${type}-${feature.id}`}
        className={`feature-item ${isSelected ? 'selected' : ''}`}
        onClick={() => handleFeatureClick(type, feature)}
      >
        <div className="feature-name">{feature.name}</div>
        {hasDescription && (
          <div className="feature-description" style={{ 
            fontSize: '0.8rem', 
            color: '#999', 
            marginTop: '2px',
            fontStyle: 'italic'
          }}>
            {(feature as any).description}
          </div>
        )}
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
            {(data as Line).description && (
              <div className="detail-row">
                <span className="detail-label">Description:</span>
                <span className="detail-value">{(data as Line).description}</span>
              </div>
            )}
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
            {(data as Polygon).description && (
              <div className="detail-row">
                <span className="detail-label">Description:</span>
                <span className="detail-value">{(data as Polygon).description}</span>
              </div>
            )}
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

        <div style={{ display: 'flex', gap: '0.5rem', marginTop: '1rem' }}>
          <button 
            className="edit-button" 
            onClick={() => handleEdit()}
            style={{
              background: '#28a745',
              color: 'white',
              border: 'none',
              padding: '0.5rem 1rem',
              borderRadius: '4px',
              cursor: 'pointer',
              fontSize: '0.875rem',
              flex: 1
            }}
          >
            Edit {type}
          </button>
          <button 
            className="delete-button" 
            onClick={handleDelete}
            style={{ flex: 1 }}
          >
            Delete {type}
          </button>
        </div>
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

      <EditFeatureModal
        isOpen={showEditModal}
        feature={editingFeature?.feature || null}
        featureType={editingFeature?.type || 'point'}
        allFeatures={{ points, lines, polygons }}
        onSubmit={handleEditSubmit}
        onCancel={handleEditCancel}
      />
    </div>
  );
};

export default Sidebar; 