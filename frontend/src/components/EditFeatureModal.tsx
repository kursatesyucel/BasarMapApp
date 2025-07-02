import React, { useState, useEffect } from 'react';
import { Point, Line, Polygon, UpdatePointDto, UpdateLineDto, UpdatePolygonDto } from '../types';

interface EditFeatureModalProps {
  isOpen: boolean;
  feature: Point | Line | Polygon | null;
  featureType: 'point' | 'line' | 'polygon';
  allFeatures: {
    points: Point[];
    lines: Line[];
    polygons: Polygon[];
  };
  onSubmit: (data: UpdatePointDto | UpdateLineDto | UpdatePolygonDto) => Promise<boolean>;
  onCancel: () => void;
}

const EditFeatureModal: React.FC<EditFeatureModalProps> = ({
  isOpen,
  feature,
  featureType,
  allFeatures,
  onSubmit,
  onCancel
}) => {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [coordinates, setCoordinates] = useState('');
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<string[]>([]);

  useEffect(() => {
    if (feature) {
      setName(feature.name);
      setDescription((feature as any).description || '');
      
      // Format coordinates based on feature type
      if (featureType === 'point') {
        const point = feature as Point;
        setCoordinates(`${point.latitude}, ${point.longitude}`);
      } else if (featureType === 'line') {
        const line = feature as Line;
        const coordStr = line.coordinates.map(coord => `${coord[1]}, ${coord[0]}`).join('\n');
        setCoordinates(coordStr);
      } else if (featureType === 'polygon') {
        const polygon = feature as Polygon;
        const coordStr = polygon.coordinates[0]?.map(coord => `${coord[1]}, ${coord[0]}`).join('\n') || '';
        setCoordinates(coordStr);
      }
    }
    setErrors([]);
  }, [feature, featureType]);

  const validateInputs = (): string[] => {
    const validationErrors: string[] = [];

    // Name validation
    if (!name.trim()) {
      validationErrors.push('Name is required');
    } else {
      // Check for duplicate names (excluding current feature)
      const isDuplicateName = 
        allFeatures.points.some(p => p.id !== feature?.id && p.name === name.trim()) ||
        allFeatures.lines.some(l => l.id !== feature?.id && l.name === name.trim()) ||
        allFeatures.polygons.some(p => p.id !== feature?.id && p.name === name.trim());
      
      if (isDuplicateName) {
        validationErrors.push('A feature with this name already exists. Please choose a different name.');
      }
    }

    // Coordinates validation
    if (!coordinates.trim()) {
      validationErrors.push('Coordinates are required');
    } else {
      try {
        if (featureType === 'point') {
          const parts = coordinates.trim().split(',');
          if (parts.length !== 2) {
            validationErrors.push('Point coordinates must be in format: latitude, longitude');
          } else {
            const lat = parseFloat(parts[0].trim());
            const lng = parseFloat(parts[1].trim());
            if (isNaN(lat) || isNaN(lng)) {
              validationErrors.push('Invalid coordinate values. Please enter valid numbers.');
            } else if (lat < -90 || lat > 90) {
              validationErrors.push('Latitude must be between -90 and 90');
            } else if (lng < -180 || lng > 180) {
              validationErrors.push('Longitude must be between -180 and 180');
            }
          }
        } else if (featureType === 'line') {
          const lines = coordinates.trim().split('\n').filter(line => line.trim());
          if (lines.length < 2) {
            validationErrors.push('Line must have at least 2 coordinate points');
          } else {
            lines.forEach((line, index) => {
              const parts = line.trim().split(',');
              if (parts.length !== 2) {
                validationErrors.push(`Line ${index + 1}: Invalid format. Use: latitude, longitude`);
              } else {
                const lat = parseFloat(parts[0].trim());
                const lng = parseFloat(parts[1].trim());
                if (isNaN(lat) || isNaN(lng)) {
                  validationErrors.push(`Line ${index + 1}: Invalid coordinate values`);
                } else if (lat < -90 || lat > 90 || lng < -180 || lng > 180) {
                  validationErrors.push(`Line ${index + 1}: Coordinates out of valid range`);
                }
              }
            });
          }
        } else if (featureType === 'polygon') {
          const lines = coordinates.trim().split('\n').filter(line => line.trim());
          if (lines.length < 3) {
            validationErrors.push('Polygon must have at least 3 coordinate points');
          } else {
            lines.forEach((line, index) => {
              const parts = line.trim().split(',');
              if (parts.length !== 2) {
                validationErrors.push(`Point ${index + 1}: Invalid format. Use: latitude, longitude`);
              } else {
                const lat = parseFloat(parts[0].trim());
                const lng = parseFloat(parts[1].trim());
                if (isNaN(lat) || isNaN(lng)) {
                  validationErrors.push(`Point ${index + 1}: Invalid coordinate values`);
                } else if (lat < -90 || lat > 90 || lng < -180 || lng > 180) {
                  validationErrors.push(`Point ${index + 1}: Coordinates out of valid range`);
                }
              }
            });
          }
        }
      } catch (error) {
        validationErrors.push('Invalid coordinates format');
      }
    }

    return validationErrors;
  };

  const parseCoordinates = () => {
    if (featureType === 'point') {
      const parts = coordinates.trim().split(',');
      const lat = parseFloat(parts[0].trim());
      const lng = parseFloat(parts[1].trim());
      return { latitude: lat, longitude: lng };
    } else if (featureType === 'line') {
      const lines = coordinates.trim().split('\n').filter(line => line.trim());
      const coordArray = lines.map(line => {
        const parts = line.trim().split(',');
        const lat = parseFloat(parts[0].trim());
        const lng = parseFloat(parts[1].trim());
        return [lng, lat]; // Backend expects [lng, lat]
      });
      return coordArray;
    } else if (featureType === 'polygon') {
      const lines = coordinates.trim().split('\n').filter(line => line.trim());
      const coordArray = lines.map(line => {
        const parts = line.trim().split(',');
        const lat = parseFloat(parts[0].trim());
        const lng = parseFloat(parts[1].trim());
        return [lng, lat]; // Backend expects [lng, lat]
      });
      
      // Ensure polygon is closed (first point = last point)
      if (coordArray.length > 0 && 
          (coordArray[0][0] !== coordArray[coordArray.length - 1][0] || 
           coordArray[0][1] !== coordArray[coordArray.length - 1][1])) {
        coordArray.push([coordArray[0][0], coordArray[0][1]]);
      }
      
      return [coordArray]; // Polygon coordinates are array of arrays
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    const validationErrors = validateInputs();
    if (validationErrors.length > 0) {
      setErrors(validationErrors);
      return;
    }

    setLoading(true);
    setErrors([]);

    try {
      let updateData: any;
      
      if (featureType === 'point') {
        const coords = parseCoordinates() as { latitude: number; longitude: number };
        updateData = {
          name: name.trim(),
          description: description.trim(),
          latitude: coords.latitude,
          longitude: coords.longitude
        };
      } else if (featureType === 'line') {
        updateData = {
          name: name.trim(),
          description: description.trim(),
          coordinates: parseCoordinates()
        };
      } else if (featureType === 'polygon') {
        updateData = {
          name: name.trim(),
          description: description.trim(),
          coordinates: parseCoordinates()
        };
      }

      const success = await onSubmit(updateData);
      if (success) {
        onCancel(); // Close modal on success
      } else {
        setErrors(['Failed to update feature. Please try again.']);
      }
    } catch (error) {
      console.error('Error updating feature:', error);
      setErrors(['An error occurred while updating the feature.']);
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen || !feature) return null;

  return (
    <div className="feature-form-overlay">
      <div className="feature-form" style={{ maxWidth: '500px', maxHeight: '80vh', overflowY: 'auto' }}>
        <div className="feature-form-header">
          <h3>Edit {featureType.charAt(0).toUpperCase() + featureType.slice(1)}</h3>
          <button className="close-button" onClick={onCancel}>×</button>
        </div>
        
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="edit-name">Name *</label>
            <input
              id="edit-name"
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Enter feature name"
              required
              autoFocus
            />
          </div>

          <div className="form-group">
            <label htmlFor="edit-description">Description</label>
            <textarea
              id="edit-description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Enter description (optional)"
              rows={3}
            />
          </div>

          <div className="form-group">
            <label htmlFor="edit-coordinates">
              Coordinates * 
              {featureType === 'point' && (
                <small style={{ display: 'block', color: '#666', fontSize: '0.8em' }}>
                  Format: latitude, longitude (e.g., 39.123456, 35.123456)
                </small>
              )}
              {featureType === 'line' && (
                <small style={{ display: 'block', color: '#666', fontSize: '0.8em' }}>
                  Format: One coordinate per line (latitude, longitude)
                </small>
              )}
              {featureType === 'polygon' && (
                <small style={{ display: 'block', color: '#666', fontSize: '0.8em' }}>
                  Format: One coordinate per line (latitude, longitude)
                </small>
              )}
            </label>
            <textarea
              id="edit-coordinates"
              value={coordinates}
              onChange={(e) => setCoordinates(e.target.value)}
              placeholder={
                featureType === 'point' 
                  ? '39.123456, 35.123456'
                  : featureType === 'line'
                  ? '39.123456, 35.123456\n39.124456, 35.124456\n39.125456, 35.125456'
                  : '39.123456, 35.123456\n39.124456, 35.124456\n39.125456, 35.125456\n39.123456, 35.123456'
              }
              rows={featureType === 'point' ? 2 : 6}
              required
              style={{ fontFamily: 'monospace', fontSize: '0.9em' }}
            />
          </div>

          <div className="form-group" style={{ marginBottom: '0.5rem' }}>
            <label style={{ color: '#666', fontSize: '0.9em' }}>
              Feature ID: {feature.id} (Read-only)
            </label>
          </div>

          {errors.length > 0 && (
            <div style={{ 
              backgroundColor: '#fee', 
              border: '1px solid #fcc', 
              borderRadius: '4px', 
              padding: '0.75rem', 
              marginBottom: '1rem' 
            }}>
              <h4 style={{ margin: '0 0 0.5rem 0', color: '#c33' }}>Validation Errors:</h4>
              <ul style={{ margin: 0, paddingLeft: '1.2rem' }}>
                {errors.map((error, index) => (
                  <li key={index} style={{ color: '#c33', fontSize: '0.9em' }}>{error}</li>
                ))}
              </ul>
            </div>
          )}

          <div className="form-actions">
            <button type="button" onClick={onCancel} disabled={loading}>
              Cancel
            </button>
            <button type="submit" disabled={loading || !name.trim() || !coordinates.trim()}>
              {loading ? 'Updating...' : 'Update'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default EditFeatureModal; 