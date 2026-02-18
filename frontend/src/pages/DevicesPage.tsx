import React, { useState, useEffect } from 'react';
import { deviceService } from '../services/deviceService';
import type { UserDevice } from '../types/deviceTypes';
import Header from '../components/Header';

const DevicesPage: React.FC = () => {
  const [devices, setDevices] = useState<UserDevice[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');

  useEffect(() => {
    loadDevices();
  }, []);

  const loadDevices = async () => {
    try {
      setLoading(true);
      setError('');
      const data = await deviceService.getMyDevices();
      setDevices(data);
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Cihazlar yüklenirken hata oluştu';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handleRevokeDevice = async (deviceId: string, deviceName: string) => {
    if (!confirm(`"${deviceName}" cihazını kaldırmak istediğinize emin misiniz? Bu cihazdan yapılan oturumlar sonlandırılacaktır.`)) {
      return;
    }

    try {
      setError('');
      setSuccessMessage('');
      const message = await deviceService.revokeDevice(deviceId);
      setSuccessMessage(message);
      // Reload devices list
      await loadDevices();
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Cihaz kaldırılırken hata oluştu';
      setError(errorMessage);
    }
  };

  const handleSetTrustedDevice = async (deviceId: string, deviceName: string) => {
    if (!confirm(`"${deviceName}" cihazını güvenli cihaz olarak işaretlemek istediğinize emin misiniz? Diğer tüm cihazlar güvenli olmayan olarak işaretlenecektir.`)) {
      return;
    }

    try {
      setError('');
      setSuccessMessage('');
      const message = await deviceService.setTrustedDevice(deviceId);
      setSuccessMessage(message);
      // Reload devices list
      await loadDevices();
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Güvenli cihaz ayarlanırken hata oluştu';
      setError(errorMessage);
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleString('tr-TR', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  return (
    <div className="app-wrapper">
      <Header />
      <div className="devices-page">
        <div className="devices-container">
          <div className="devices-header">
            <h1>Cihazlarım</h1>
            <p className="devices-description">
              Hesabınıza giriş yapmış olan tüm cihazları buradan yönetebilirsiniz.
              Tanımadığınız bir cihaz görürseniz, hemen kaldırın ve şifrenizi değiştirin.
            </p>
          </div>

          {error && (
            <div className="devices-error">
              {error}
            </div>
          )}

          {successMessage && (
            <div className="devices-success">
              {successMessage}
            </div>
          )}

          {loading ? (
            <div className="devices-loading">
              <div className="spinner"></div>
              <p>Cihazlar yükleniyor...</p>
            </div>
          ) : devices.length === 0 ? (
            <div className="devices-empty">
              <p>Henüz kayıtlı cihaz bulunmuyor.</p>
            </div>
          ) : (
            <div className="devices-list">
              {devices.map((device) => (
                <div
                  key={device.id}
                  className={`device-card ${device.isCurrentDevice ? 'current-device' : ''}`}
                >
                  <div className="device-icon">
                    {device.deviceName.includes('Windows') || device.deviceName.includes('Linux') || device.deviceName.includes('macOS') ? '💻' : '📱'}
                  </div>
                  <div className="device-info">
                    <div className="device-name">
                      {device.deviceName}
                      {device.isCurrentDevice && (
                        <span className="current-badge">Bu Cihaz</span>
                      )}
                      {device.isTrusted && (
                        <span className="trusted-badge">🔒 Güvenli Cihaz</span>
                      )}
                    </div>
                    <div className="device-details">
                      <div className="device-detail">
                        <span className="detail-label">IP Adresi:</span>
                        <span className="detail-value">{device.ipAddress}</span>
                      </div>
                      <div className="device-detail">
                        <span className="detail-label">Son Giriş:</span>
                        <span className="detail-value">{formatDate(device.lastLoginDate)}</span>
                      </div>
                      <div className="device-detail">
                        <span className="detail-label">İlk Görülme:</span>
                        <span className="detail-value">{formatDate(device.firstSeenDate)}</span>
                      </div>
                    </div>
                  </div>
                  <div className="device-actions">
                    {!device.isTrusted && (
                      <button
                        onClick={() => handleSetTrustedDevice(device.deviceId, device.deviceName)}
                        className="trust-button"
                        title="Güvenli Cihaz Yap"
                      >
                        🔒 Güvenli Cihaz Yap
                      </button>
                    )}
                    {!device.isCurrentDevice && (
                      <button
                        onClick={() => handleRevokeDevice(device.deviceId, device.deviceName)}
                        className="revoke-button"
                        title="Cihazı Kaldır"
                      >
                        🗑️ Kaldır
                      </button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}

          <div className="devices-footer">
            <div className="security-tip">
              <strong>💡 Güvenlik İpucu:</strong> Güvenli cihaz olarak işaretlediğiniz cihaz dışındaki tüm cihazlardan giriş yaptığınızda e-posta ile bildirim alırsınız.
              Tanımadığınız veya artık kullanmadığınız cihazları düzenli olarak kaldırın.
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default DevicesPage;
