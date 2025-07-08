import React, { useState, useEffect } from 'react';
import { Point, Line, Polygon, Camera, CreatePointDto, CreateLineDto, CreatePolygonDto, CreateCameraDto } from '../types';

interface FeatureFormProps {
  isOpen: boolean;
  featureType: 'point' | 'line' | 'polygon' | 'camera';
  initialData?: Point | Line | Polygon | Camera;
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
  const [videoFileName, setVideoFileName] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (initialData) {
      setName(initialData.name);
      if ('description' in initialData) {
        setDescription(initialData.description || '');
      }
      if (featureType === 'camera' && 'videoFileName' in initialData) {
        setVideoFileName(initialData.videoFileName || '');
      }
    } else {
      setName('');
      setDescription('');
      setVideoFileName('');
    }
  }, [initialData, featureType]);

  const handleSubmit = async (e: React.FormEvent) => {
    console.log('FeatureForm.handleSubmit called');
    e.preventDefault();
    
    if (!name.trim()) {
      console.log('Name is empty, returning');
      return;
    }

    if (featureType === 'camera' && !videoFileName.trim()) {
      console.log('Video file name is empty for camera, returning');
      return;
    }

    console.log('Setting loading to true');
    setLoading(true);
    
    try {
      if (featureType === 'camera') {
        console.log(`Submitting ${featureType} with:`, { name, description, videoFileName });
      } else {
        console.log(`Submitting ${featureType} with:`, { name, description });
      }
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
      } else if (featureType === 'camera') {
        console.log('Calling onSubmit for camera');
        await onSubmit({ name, description, videoFileName, isActive: true });
        console.log('onSubmit completed for camera');
      }
      
      console.log('Form submitted successfully');
      setName('');
      setDescription('');
      setVideoFileName('');
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

  console.log('FeatureForm is rendering with:', { isOpen, featureType, name, description, videoFileName });

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

          {featureType === 'camera' && (
            <>
              <div className="form-group">
                <label htmlFor="videoFileName">Video File Name *</label>
                <input
                  id="videoFileName"
                  type="text"
                  value={videoFileName}
                  onChange={(e) => setVideoFileName(e.target.value)}
                  placeholder="e.g. Video_1.mp4, 10hr_Video.mp4"
                  required
                />
                <small className="field-hint">
                  Enter the video file name that exists in the cameras folder
                </small>
              </div>


            </>
          )}

          <div className="form-actions">
            <button type="button" onClick={onCancel} disabled={loading}>
              Cancel
            </button>
            <button 
              type="submit" 
              disabled={loading || !name.trim() || (featureType === 'camera' && !videoFileName.trim())}
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