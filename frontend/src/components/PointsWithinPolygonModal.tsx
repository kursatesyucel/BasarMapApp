import React from 'react';
import { Point } from '../types';

interface PointsWithinPolygonModalProps {
  isOpen: boolean;
  points: Point[];
  onClose: () => void;
  polygonName?: string;
  showEditConfirmation?: boolean;
  onConfirmEdit?: () => void;
  onCancelEdit?: () => void;
}

const PointsWithinPolygonModal: React.FC<PointsWithinPolygonModalProps> = ({
  isOpen,
  points,
  onClose,
  polygonName = 'Selected Polygon',
  showEditConfirmation = false,
  onConfirmEdit,
  onCancelEdit
}) => {
  if (!isOpen) return null;

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
    noPointsMessage: {
      textAlign: 'center' as const,
      color: '#64748b',
      fontStyle: 'italic' as const,
      margin: '40px 0',
    },
    pointsCount: {
      marginBottom: '16px',
      fontWeight: 500,
      color: '#1e293b',
    },
    pointsList: {
      display: 'flex',
      flexDirection: 'column' as const,
      gap: '16px',
    },
    pointItem: {
      border: '1px solid #e2e8f0',
      borderRadius: '6px',
      padding: '16px',
      backgroundColor: '#f8fafc',
    },
    pointHeader: {
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      marginBottom: '12px',
    },
    pointName: {
      margin: 0,
      color: '#1e293b',
      fontSize: '1.1rem',
    },
    pointId: {
      backgroundColor: '#e2e8f0',
      color: '#475569',
      padding: '4px 8px',
      borderRadius: '4px',
      fontSize: '0.875rem',
      fontFamily: 'monospace',
    },
    pointCoordinates: {
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
    pointDescription: {
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
    pointTimestamps: {
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
  };

  return (
    <div style={styles.modalOverlay} onClick={onClose}>
      <div style={styles.modalContent} onClick={(e) => e.stopPropagation()}>
        <div style={styles.modalHeader}>
          <h3 style={styles.modalTitle}>
            {showEditConfirmation ? `Confirm Edit: Points within ${polygonName}` : `Points within ${polygonName}`}
          </h3>
          <button
            style={styles.modalClose}
            onClick={onClose}
            onMouseEnter={(e) => (e.currentTarget.style.color = '#e11d48')}
            onMouseLeave={(e) => (e.currentTarget.style.color = '#64748b')}
          >
            ×
          </button>
        </div>
        
        <div style={styles.modalBody}>
          {points.length === 0 ? (
            <p style={styles.noPointsMessage}>No points found within this polygon.</p>
          ) : (
            <>
              {showEditConfirmation && (
                <div style={{
                  marginBottom: '16px',
                  padding: '12px',
                  backgroundColor: '#fef3c7',
                  border: '1px solid #fcd34d',
                  borderRadius: '6px',
                  color: '#d97706',
                  fontWeight: 500,
                }}>
                  ⚠️ Warning: This edit will affect {points.length} point(s) within the polygon area.
                </div>
              )}
              <p style={styles.pointsCount}>
                {showEditConfirmation 
                  ? 'The following points will be affected by your polygon edit. Do you want to proceed?'
                  : `Found ${points.length} point(s) within this polygon:`
                }
              </p>
              <div style={styles.pointsList}>
                {points.map((point) => (
                  <div key={point.id} style={styles.pointItem}>
                    <div style={styles.pointHeader}>
                      <h4 style={styles.pointName}>{point.name}</h4>
                      <span style={styles.pointId}>ID: {point.id}</span>
                    </div>
                    
                    <div style={styles.pointCoordinates}>
                      <div style={styles.coordinate}>
                        <label style={styles.coordinateLabel}>Latitude:</label>
                        <span style={styles.coordinateValue}>{point.latitude.toFixed(6)}</span>
                      </div>
                      <div style={styles.coordinate}>
                        <label style={styles.coordinateLabel}>Longitude:</label>
                        <span style={styles.coordinateValue}>{point.longitude.toFixed(6)}</span>
                      </div>
                    </div>
                    
                    {point.description && (
                      <div style={styles.pointDescription}>
                        <label style={styles.descriptionLabel}>Description:</label>
                        <p style={styles.descriptionText}>{point.description}</p>
                      </div>
                    )}
                    
                    <div style={styles.pointTimestamps}>
                      <div style={styles.timestamp}>
                        <label style={styles.timestampLabel}>Created:</label>
                        <span style={styles.timestampValue}>{new Date(point.createdAt).toLocaleString()}</span>
                      </div>
                      {point.updatedAt && (
                        <div style={styles.timestamp}>
                          <label style={styles.timestampLabel}>Updated:</label>
                          <span style={styles.timestampValue}>{new Date(point.updatedAt).toLocaleString()}</span>
                        </div>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </>
          )}
        </div>
        
        <div style={styles.modalFooter}>
          {showEditConfirmation ? (
            <>
              <button 
                style={{...styles.btnSecondary, backgroundColor: '#ef4444'}} 
                onClick={onCancelEdit}
                onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = '#dc2626')}
                onMouseLeave={(e) => (e.currentTarget.style.backgroundColor = '#ef4444')}
              >
                Cancel Edit
              </button>
              <button 
                style={{...styles.btnSecondary, backgroundColor: '#3b82f6'}} 
                onClick={onConfirmEdit}
                onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = '#2563eb')}
                onMouseLeave={(e) => (e.currentTarget.style.backgroundColor = '#3b82f6')}
              >
                Proceed with Edit
              </button>
            </>
          ) : (
            <button 
              style={styles.btnSecondary} 
              onClick={onClose}
              onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = '#475569')}
              onMouseLeave={(e) => (e.currentTarget.style.backgroundColor = '#64748b')}
            >
              Close
            </button>
          )}
        </div>
      </div>
    </div>
  );
};

export default PointsWithinPolygonModal; 