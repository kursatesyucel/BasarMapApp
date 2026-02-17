import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams, Link } from 'react-router-dom';
import { authService } from '../services/authService';

const ResetPassword: React.FC = () => {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') || '';
  
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    if (!token) {
      setError('Geçersiz veya eksik sıfırlama linki. Lütfen şifre sıfırlama talebinde bulunun.');
    }
  }, [token]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (newPassword !== confirmPassword) {
      setError('Şifreler eşleşmiyor.');
      return;
    }

    if (newPassword.length < 6) {
      setError('Şifre en az 6 karakter olmalıdır.');
      return;
    }

    try {
      setLoading(true);
      const message = await authService.resetPassword({
        token,
        newPassword,
        confirmPassword,
      });
      setSuccess(message);

      setTimeout(() => {
        navigate('/login', { state: { message: 'Şifreniz güncellendi. Yeni şifre ile giriş yapabilirsiniz.' } });
      }, 2500);
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Bir hata oluştu. Lütfen yeni bir sıfırlama talebi oluşturun.';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  if (!token) {
    return (
      <div className="auth-container">
        <div className="auth-card">
          <h2 className="auth-title">Şifre Sıfırlama</h2>
          <div className="auth-error">{error}</div>
          <p className="auth-footer">
            <Link to="/forgot-password">Şifremi unuttum</Link> | <Link to="/login">Giriş yap</Link>
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="auth-container">
      <div className="auth-card">
        <div className="verify-icon">🔑</div>
        <h2 className="auth-title">Yeni Şifre Belirle</h2>

        <p className="verify-instruction">
          Hesabınız için yeni bir şifre girin.
        </p>

        {error && (
          <div className="auth-error">
            {error}
          </div>
        )}

        {success && (
          <div className="auth-success">
            {success}
            <div className="success-redirect">Giriş sayfasına yönlendiriliyorsunuz...</div>
          </div>
        )}

        {!success && (
          <form onSubmit={handleSubmit} className="auth-form">
            <div className="form-group">
              <label htmlFor="newPassword">Yeni Şifre</label>
              <input
                id="newPassword"
                type="password"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                required
                minLength={6}
                disabled={loading}
                placeholder="En az 6 karakter"
                autoComplete="new-password"
                autoFocus
              />
              <small className="input-hint">En az 6 karakter</small>
            </div>

            <div className="form-group">
              <label htmlFor="confirmPassword">Şifre Tekrar</label>
              <input
                id="confirmPassword"
                type="password"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                required
                minLength={6}
                disabled={loading}
                placeholder="Şifreyi tekrar girin"
                autoComplete="new-password"
              />
            </div>

            <button type="submit" disabled={loading} className="auth-button">
              {loading ? 'Kaydediliyor...' : 'Şifreyi Güncelle'}
            </button>
          </form>
        )}

        <p className="auth-footer">
          <Link to="/login">Giriş sayfasına dön</Link>
        </p>
      </div>
    </div>
  );
};

export default ResetPassword;
