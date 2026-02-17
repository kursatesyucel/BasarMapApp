import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { authService } from '../services/authService';

const ForgotPassword: React.FC = () => {
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    try {
      setLoading(true);
      const message = await authService.forgotPassword({ email });
      setSuccess(message);
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Bir hata oluştu. Lütfen daha sonra tekrar deneyin.';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <div className="verify-icon">🔐</div>
        <h2 className="auth-title">Şifremi Unuttum</h2>

        <p className="verify-instruction">
          E-posta adresinizi girin, size şifre sıfırlama linki göndereceğiz.
        </p>

        {error && (
          <div className="auth-error">
            {error}
          </div>
        )}

        {success && (
          <div className="auth-success">
            {success}
            <div className="auth-info">
              <small>E-postayı kontrol edin. Gelen kutunuzda yoksa spam klasörüne bakın. Link 15 dakika geçerlidir.</small>
            </div>
          </div>
        )}

        {!success && (
          <form onSubmit={handleSubmit} className="auth-form">
            <div className="form-group">
              <label htmlFor="email">E-posta Adresi</label>
              <input
                id="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                disabled={loading}
                placeholder="ornek@email.com"
                autoComplete="email"
                autoFocus
              />
            </div>

            <button type="submit" disabled={loading} className="auth-button">
              {loading ? 'Gönderiliyor...' : 'Sıfırlama Linki Gönder'}
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

export default ForgotPassword;
