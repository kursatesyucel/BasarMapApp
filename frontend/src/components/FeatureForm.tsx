import React, { useState, useEffect } from 'react';
import { Point, Line, Polygon, CreatePointDto, CreateLineDto, CreatePolygonDto } from '../types';

interface FeatureFormProps {
  isOpen: boolean;
  featureType: 'point' | 'line' | 'polygon';
  initialData?: Point | Line | Polygon;
  onSubmit: (data: any) => Promise<void>;
  onCancel: () => void;
  title: string;
}

const FeatureForm: React.FC<FeatureFormProps> = ({
  isOpen,
  featureType,
  initialData,
  onSubmit,
  onCancel,
  title
}) => {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (initialData) {
      setName(initialData.name);
      if ('description' in initialData) {
        setDescription(initialData.description || '');
      }
    } else {
      setName('');
      setDescription('');
    }
  }, [initialData, featureType]);

  const handleSubmit = async (e: React.FormEvent) => {
    console.log('FeatureForm.handleSubmit called');
    e.preventDefault();
    
    if (!name.trim()) {
      console.log('Name is empty, returning');
      return;
    }

    console.log('Setting loading to true');
    setLoading(true);
    
    try {
      console.log(`Submitting ${featureType} with:`, { name, description });
      console.log('onSubmit prop:', onSubmit);
      
      if (featureType === 'point') {
        console.log('Calling onSubmit for point');
        await onSubmit({ name, description });
        console.log('onSubmit completed for point');
      } else if (featureType === 'line') {
        console.log('Calling onSubmit for line');
        await onSubmit({ name, description });
        console.log('onSubmit completed for line');
      } else if (featureType === 'polygon') {
        console.log('Calling onSubmit for polygon');
        await onSubmit({ name, description });
        console.log('onSubmit completed for polygon');
      }
      
      console.log('Form submitted successfully');
      setName('');
      setDescription('');
    } catch (error) {
      console.error('Error submitting form:', error);
    } finally {
      console.log('Setting loading to false');
      setLoading(false);
    }
  };

  if (!isOpen) {
    console.log('FeatureForm is not open');
    return null;
  }

  console.log('FeatureForm is rendering with:', { isOpen, featureType, name, description });

  return (
    <div className="feature-form-overlay">
      <div className="feature-form">
        <div className="feature-form-header">
          <h3>{title}</h3>
          <button className="close-button" onClick={onCancel}>×</button>
        </div>
        
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="name">Name *</label>
            <input
              id="name"
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder={`Enter ${featureType} name`}
              required
              autoFocus
            />
          </div>

          <div className="form-group">
            <label htmlFor="description">Description</label>
            <textarea
              id="description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Enter description (optional)"
              rows={3}
            />
          </div>

          <div className="form-actions">
            <button type="button" onClick={onCancel} disabled={loading}>
              Cancel
            </button>
            <button 
              type="submit" 
              disabled={loading || !name.trim()}
              onClick={() => console.log('Save button clicked!')}
            >
              {loading ? 'Saving...' : 'Save'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default FeatureForm; 