import React from 'react';
import { Camera } from '../types';
import VideoPlayer from './VideoPlayer';
import { cameraService } from '../services/cameraService';

interface CameraPopupProps {
  camera: Camera;
  isOpen: boolean;
  onClose: () => void;
}

const CameraPopup: React.FC<CameraPopupProps> = ({ camera, isOpen, onClose }) => {
  if (!isOpen) return null;

  const handleBackdropClick = (e: React.MouseEvent) => {
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  const handleVideoError = (error: string) => {
    console.error('Video loading error:', error);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('tr-TR');
  };

  const overlayStyle: React.CSSProperties = {
    position: 'fixed',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    backgroundColor: 'rgba(0, 0, 0, 0.7)',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    zIndex: 1000,
    padding: '20px'
  };

  const popupStyle: React.CSSProperties = {
    backgroundColor: 'white',
    borderRadius: '12px',
    maxWidth: '800px',
    width: '100%',
    maxHeight: '90vh',
    overflow: 'auto',
    boxShadow: '0 10px 25px rgba(0, 0, 0, 0.3)',
    position: 'relative'
  };

  const headerStyle: React.CSSProperties = {
    padding: '20px 24px 16px',
    borderBottom: '1px solid #e5e5e5',
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'flex-start'
  };

  const titleStyle: React.CSSProperties = {
    margin: 0,
    fontSize: '20px',
    fontWeight: '600',
    color: '#333',
    flex: 1
  };

  const closeButtonStyle: React.CSSProperties = {
    background: 'none',
    border: 'none',
    fontSize: '24px',
    cursor: 'pointer',
    color: '#666',
    padding: '4px',
    borderRadius: '4px',
    marginLeft: '16px',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    width: '32px',
    height: '32px'
  };

  const contentStyle: React.CSSProperties = {
    padding: '20px 24px'
  };

  const infoSectionStyle: React.CSSProperties = {
    marginBottom: '20px'
  };

  const descriptionStyle: React.CSSProperties = {
    color: '#666',
    fontSize: '14px',
    lineHeight: '1.5',
    margin: '8px 0 0'
  };

  const statusStyle: React.CSSProperties = {
    display: 'inline-flex',
    alignItems: 'center',
    gap: '6px',
    padding: '4px 8px',
    borderRadius: '12px',
    fontSize: '12px',
    fontWeight: '500',
    backgroundColor: camera.isActive ? '#e8f5e8' : '#f5f5f5',
    color: camera.isActive ? '#2e7d32' : '#666',
    marginTop: '8px'
  };

  const videoSectionStyle: React.CSSProperties = {
    marginTop: '20px'
  };

  const videoTitleStyle: React.CSSProperties = {
    fontSize: '16px',
    fontWeight: '600',
    color: '#333',
    marginBottom: '12px'
  };

  const infoItemStyle: React.CSSProperties = {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: '8px 0',
    fontSize: '14px',
    borderBottom: '1px solid #f5f5f5'
  };

  const labelStyle: React.CSSProperties = {
    color: '#666',
    fontWeight: '500'
  };

  const valueStyle: React.CSSProperties = {
    color: '#333'
  };

  return (
    <div style={overlayStyle} onClick={handleBackdropClick}>
      <div style={popupStyle} onClick={(e) => e.stopPropagation()}>
        {/* Header */}
        <div style={headerStyle}>
          <h2 style={titleStyle}>{camera.name}</h2>
          <button 
            style={closeButtonStyle} 
            onClick={onClose}
            onMouseOver={(e) => e.currentTarget.style.backgroundColor = '#f5f5f5'}
            onMouseOut={(e) => e.currentTarget.style.backgroundColor = 'transparent'}
          >
            ×
          </button>
        </div>

        {/* Content */}
        <div style={contentStyle}>
          {/* Camera Info */}
          <div style={infoSectionStyle}>
            {camera.description && (
              <p style={descriptionStyle}>{camera.description}</p>
            )}
            
            <div style={statusStyle}>
              <span style={{ 
                width: '8px', 
                height: '8px', 
                borderRadius: '50%', 
                backgroundColor: camera.isActive ? '#4caf50' : '#999' 
              }} />
              {camera.isActive ? 'Aktif' : 'Pasif'}
            </div>

            {/* Detailed Info */}
            <div style={{ marginTop: '16px' }}>
              <div style={infoItemStyle}>
                <span style={labelStyle}>Koordinatlar:</span>
                <span style={valueStyle}>
                  {camera.latitude.toFixed(6)}, {camera.longitude.toFixed(6)}
                </span>
              </div>
              
              <div style={infoItemStyle}>
                <span style={labelStyle}>Video Dosyası:</span>
                <span style={valueStyle}>{camera.videoFileName}</span>
              </div>
              
              <div style={infoItemStyle}>
                <span style={labelStyle}>Oluşturma Tarihi:</span>
                <span style={valueStyle}>{formatDate(camera.createdAt)}</span>
              </div>
              
              {camera.updatedAt && (
                <div style={infoItemStyle}>
                  <span style={labelStyle}>Güncelleme Tarihi:</span>
                  <span style={valueStyle}>{formatDate(camera.updatedAt)}</span>
                </div>
              )}
            </div>
          </div>

          {/* Video Section */}
          {camera.isActive && (
            <div style={videoSectionStyle}>
              <h3 style={videoTitleStyle}>Canlı Görüntü</h3>
              <VideoPlayer
                videoUrl={cameraService.getVideoStreamUrl(camera.videoFileName)}
                width="100%"
                height="400px"
                controls={true}
                muted={true}
                onError={handleVideoError}
                onLoadStart={() => console.log('Video loading started for:', camera.name)}
                onCanPlay={() => console.log('Video can play:', camera.name)}
              />
            </div>
          )}

          {!camera.isActive && (
            <div style={{
              ...videoSectionStyle,
              textAlign: 'center',
              padding: '40px 20px',
              backgroundColor: '#f9f9f9',
              borderRadius: '8px',
              color: '#666'
            }}>
              <div style={{ fontSize: '48px', marginBottom: '16px' }}>📹</div>
              <p>Bu kamera şu anda aktif değil</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default CameraPopup; 