import React from 'react';
import { Point, Camera } from '../types';

interface FeaturesWithinPolygonModalProps {
  isOpen: boolean;
  points: Point[];
  cameras: Camera[];
  onClose: () => void;
  polygonName?: string;
  showEditConfirmation?: boolean;
  onConfirmEdit?: () => void;
  onCancelEdit?: () => void;
}

const FeaturesWithinPolygonModal: React.FC<FeaturesWithinPolygonModalProps> = ({
  isOpen,
  points,
  cameras,
  onClose,
  polygonName = 'Selected Polygon',
  showEditConfirmation = false,
  onConfirmEdit,
  onCancelEdit
}) => {
  if (!isOpen) return null;

  const totalFeatures = points.length + cameras.length;

  const styles = {
    modalOverlay: {
      position: 'fixed' as const,
      top: 0,
      left: 0,
      right: 0,
      bottom: 0,
      backgroundColor: 'rgba(0, 0, 0, 0.5)',
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      zIndex: 1000,
    },
    modalContent: {
      background: 'white',
      borderRadius: '8px',
      boxShadow: '0 4px 6px rgba(0, 0, 0, 0.1)',
      maxWidth: '600px',
      maxHeight: '80vh',
      width: '90%',
      overflow: 'hidden' as const,
      display: 'flex',
      flexDirection: 'column' as const,
    },
    modalHeader: {
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      padding: '20px',
      borderBottom: '1px solid #e2e8f0',
      backgroundColor: showEditConfirmation ? '#fef3c7' : '#f8fafc',
    },
    modalTitle: {
      margin: 0,
      color: '#1e293b',
      fontSize: '1.25rem',
    },
    modalClose: {
      background: 'none',
      border: 'none',
      fontSize: '24px',
      cursor: 'pointer',
      color: '#64748b',
      padding: 0,
      width: '30px',
      height: '30px',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
    },
    modalBody: {
      flex: 1,
      overflowY: 'auto' as const,
      padding: '20px',
    },
    noFeaturesMessage: {
      textAlign: 'center' as const,
      color: '#64748b',
      fontStyle: 'italic' as const,
      margin: '40px 0',
    },
    featuresCount: {
      marginBottom: '16px',
      fontWeight: 500,
      color: '#1e293b',
    },
    featuresList: {
      display: 'flex',
      flexDirection: 'column' as const,
      gap: '16px',
    },
    sectionTitle: {
      margin: '20px 0 12px 0',
      color: '#1e293b',
      fontSize: '1.1rem',
      fontWeight: 600,
      borderBottom: '2px solid #e2e8f0',
      paddingBottom: '8px',
    },
    featureItem: {
      border: '1px solid #e2e8f0',
      borderRadius: '6px',
      padding: '16px',
      backgroundColor: '#f8fafc',
    },
    cameraItem: {
      border: '1px solid #fbbf24',
      borderRadius: '6px',
      padding: '16px',
      backgroundColor: '#fef3c7',
    },
    featureHeader: {
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      marginBottom: '12px',
    },
    featureName: {
      margin: 0,
      color: '#1e293b',
      fontSize: '1.1rem',
      display: 'flex',
      alignItems: 'center',
      gap: '8px',
    },
    featureIcon: {
      fontSize: '1.2rem',
    },
    featureId: {
      backgroundColor: '#e2e8f0',
      color: '#475569',
      padding: '4px 8px',
      borderRadius: '4px',
      fontSize: '0.875rem',
      fontFamily: 'monospace',
    },
    cameraId: {
      backgroundColor: '#f59e0b',
      color: 'white',
      padding: '4px 8px',
      borderRadius: '4px',
      fontSize: '0.875rem',
      fontFamily: 'monospace',
    },
    featureCoordinates: {
      display: 'grid',
      gridTemplateColumns: '1fr 1fr',
      gap: '12px',
      marginBottom: '12px',
    },
    coordinate: {
      display: 'flex',
      flexDirection: 'column' as const,
      gap: '4px',
    },
    coordinateLabel: {
      fontWeight: 500,
      color: '#475569',
      fontSize: '0.875rem',
    },
    coordinateValue: {
      fontFamily: 'monospace',
      color: '#1e293b',
      backgroundColor: 'white',
      padding: '6px 8px',
      border: '1px solid #d1d5db',
      borderRadius: '4px',
      fontSize: '0.875rem',
    },
    featureDescription: {
      marginBottom: '12px',
    },
    descriptionLabel: {
      fontWeight: 500,
      color: '#475569',
      fontSize: '0.875rem',
      display: 'block',
      marginBottom: '4px',
    },
    descriptionText: {
      margin: 0,
      color: '#1e293b',
      backgroundColor: 'white',
      padding: '8px',
      border: '1px solid #d1d5db',
      borderRadius: '4px',
      fontSize: '0.875rem',
    },
    videoFileName: {
      marginBottom: '12px',
    },
    videoFileLabel: {
      fontWeight: 500,
      color: '#475569',
      fontSize: '0.875rem',
      display: 'block',
      marginBottom: '4px',
    },
    videoFileValue: {
      margin: 0,
      color: '#1e293b',
      backgroundColor: 'white',
      padding: '8px',
      border: '1px solid #d1d5db',
      borderRadius: '4px',
      fontSize: '0.875rem',
      fontFamily: 'monospace',
    },
    featureTimestamps: {
      display: 'grid',
      gridTemplateColumns: '1fr 1fr',
      gap: '12px',
    },
    timestamp: {
      display: 'flex',
      flexDirection: 'column' as const,
      gap: '4px',
    },
    timestampLabel: {
      fontWeight: 500,
      color: '#475569',
      fontSize: '0.75rem',
    },
    timestampValue: {
      color: '#64748b',
      fontSize: '0.75rem',
    },
    modalFooter: {
      padding: '20px',
      borderTop: '1px solid #e2e8f0',
      backgroundColor: '#f8fafc',
      display: 'flex',
      justifyContent: 'flex-end',
      gap: '12px',
    },
    btnSecondary: {
      padding: '8px 16px',
      borderRadius: '6px',
      border: 'none',
      cursor: 'pointer',
      fontWeight: 500,
      backgroundColor: '#64748b',
      color: 'white',
      transition: 'all 0.2s',
    },
    btnPrimary: {
      padding: '8px 16px',
      borderRadius: '6px',
      border: 'none',
      cursor: 'pointer',
      fontWeight: 500,
      backgroundColor: '#3b82f6',
      color: 'white',
      transition: 'all 0.2s',
    },
    btnDanger: {
      padding: '8px 16px',
      borderRadius: '6px',
      border: 'none',
      cursor: 'pointer',
      fontWeight: 500,
      backgroundColor: '#ef4444',
      color: 'white',
      transition: 'all 0.2s',
    },
    warningBox: {
      backgroundColor: '#fef3c7',
      border: '1px solid #f59e0b',
      borderRadius: '6px',
      padding: '16px',
      marginBottom: '20px',
      display: 'flex',
      alignItems: 'flex-start',
      gap: '12px',
    },
    warningIcon: {
      color: '#f59e0b',
      fontSize: '1.25rem',
      marginTop: '2px',
    },
    warningContent: {
      flex: 1,
    },
    warningTitle: {
      fontWeight: 600,
      color: '#92400e',
      margin: '0 0 8px 0',
    },
    warningText: {
      color: '#92400e',
      margin: 0,
      fontSize: '0.875rem',
    },
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };

  return (
    <div style={styles.modalOverlay} onClick={onClose}>
      <div style={styles.modalContent} onClick={(e) => e.stopPropagation()}>
        <div style={styles.modalHeader}>
          <h3 style={styles.modalTitle}>
            {showEditConfirmation ? `Confirm Edit: Features within ${polygonName}` : `Features within ${polygonName}`}
          </h3>
          <button 
            style={styles.modalClose} 
            onClick={onClose}
            aria-label="Close modal"
          >
            ×
          </button>
        </div>
        
        <div style={styles.modalBody}>
          {showEditConfirmation && totalFeatures > 0 && (
            <div style={styles.warningBox}>
              <div style={styles.warningIcon}>⚠️</div>
              <div style={styles.warningContent}>
                <div style={styles.warningTitle}>Warning: This edit will affect {totalFeatures} feature(s) within the polygon area.</div>
                <div style={styles.warningText}>
                  {totalFeatures === 1 
                    ? 'The following feature will be affected by your polygon edit. Do you want to proceed?'
                    : `Found ${totalFeatures} feature(s) within this polygon. Your edit may affect these features. Do you want to proceed?`
                  }
                </div>
              </div>
            </div>
          )}

          {totalFeatures === 0 ? (
            <p style={styles.noFeaturesMessage}>No features found within this polygon.</p>
          ) : (
            <>
              {!showEditConfirmation && (
                <div style={styles.featuresCount}>
                  Found {totalFeatures} feature(s) within this polygon:
                  {points.length > 0 && ` ${points.length} point(s)`}
                  {points.length > 0 && cameras.length > 0 && ', '}
                  {cameras.length > 0 && ` ${cameras.length} camera(s)`}
                </div>
              )}
              
              <div style={styles.featuresList}>
                {/* Points Section */}
                {points.length > 0 && (
                  <>
                    {!showEditConfirmation && <h4 style={styles.sectionTitle}>📍 Points ({points.length})</h4>}
                    {points.map((point) => (
                      <div key={`point-${point.id}`} style={styles.featureItem}>
                        <div style={styles.featureHeader}>
                          <h5 style={styles.featureName}>
                            <span style={styles.featureIcon}>📍</span>
                            {point.name}
                          </h5>
                          <span style={styles.featureId}>ID: {point.id}</span>
                        </div>
                        
                        <div style={styles.featureCoordinates}>
                          <div style={styles.coordinate}>
                            <span style={styles.coordinateLabel}>Latitude</span>
                            <span style={styles.coordinateValue}>{point.latitude.toFixed(6)}</span>
                          </div>
                          <div style={styles.coordinate}>
                            <span style={styles.coordinateLabel}>Longitude</span>
                            <span style={styles.coordinateValue}>{point.longitude.toFixed(6)}</span>
                          </div>
                        </div>
                        
                        {point.description && (
                          <div style={styles.featureDescription}>
                            <span style={styles.descriptionLabel}>Description</span>
                            <p style={styles.descriptionText}>{point.description}</p>
                          </div>
                        )}
                        
                                                 <div style={styles.featureTimestamps}>
                           <div style={styles.timestamp}>
                             <span style={styles.timestampLabel}>Created</span>
                             <span style={styles.timestampValue}>{point.createdAt ? formatDate(point.createdAt) : 'N/A'}</span>
                           </div>
                           <div style={styles.timestamp}>
                             <span style={styles.timestampLabel}>Updated</span>
                             <span style={styles.timestampValue}>{point.updatedAt ? formatDate(point.updatedAt) : 'N/A'}</span>
                           </div>
                         </div>
                      </div>
                    ))}
                  </>
                )}

                {/* Cameras Section */}
                {cameras.length > 0 && (
                  <>
                    {!showEditConfirmation && <h4 style={styles.sectionTitle}>📹 Cameras ({cameras.length})</h4>}
                    {cameras.map((camera) => (
                      <div key={`camera-${camera.id}`} style={styles.cameraItem}>
                        <div style={styles.featureHeader}>
                          <h5 style={styles.featureName}>
                            <span style={styles.featureIcon}>📹</span>
                            {camera.name}
                          </h5>
                          <span style={styles.cameraId}>ID: {camera.id}</span>
                        </div>
                        
                        <div style={styles.featureCoordinates}>
                          <div style={styles.coordinate}>
                            <span style={styles.coordinateLabel}>Latitude</span>
                            <span style={styles.coordinateValue}>{camera.latitude.toFixed(6)}</span>
                          </div>
                          <div style={styles.coordinate}>
                            <span style={styles.coordinateLabel}>Longitude</span>
                            <span style={styles.coordinateValue}>{camera.longitude.toFixed(6)}</span>
                          </div>
                        </div>
                        
                        {camera.description && (
                          <div style={styles.featureDescription}>
                            <span style={styles.descriptionLabel}>Description</span>
                            <p style={styles.descriptionText}>{camera.description}</p>
                          </div>
                        )}

                        <div style={styles.videoFileName}>
                          <span style={styles.videoFileLabel}>Video File</span>
                          <p style={styles.videoFileValue}>{camera.videoFileName}</p>
                        </div>
                        
                                                 <div style={styles.featureTimestamps}>
                           <div style={styles.timestamp}>
                             <span style={styles.timestampLabel}>Created</span>
                             <span style={styles.timestampValue}>{camera.createdAt ? formatDate(camera.createdAt) : 'N/A'}</span>
                           </div>
                           <div style={styles.timestamp}>
                             <span style={styles.timestampLabel}>Updated</span>
                             <span style={styles.timestampValue}>{camera.updatedAt ? formatDate(camera.updatedAt) : 'N/A'}</span>
                           </div>
                         </div>
                      </div>
                    ))}
                  </>
                )}
              </div>
            </>
          )}
        </div>
        
        <div style={styles.modalFooter}>
          {showEditConfirmation ? (
            <>
              <button 
                style={styles.btnSecondary} 
                onClick={onCancelEdit}
              >
                Cancel
              </button>
              <button 
                style={styles.btnDanger} 
                onClick={onConfirmEdit}
              >
                Proceed with Edit
              </button>
            </>
          ) : (
            <button 
              style={styles.btnPrimary} 
              onClick={onClose}
            >
              Close
            </button>
          )}
        </div>
      </div>
    </div>
  );
};

export default FeaturesWithinPolygonModal; 