import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { authService } from '../services/authService';

const EmailVerification: React.FC = () => {
  const [code, setCode] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const navigate = useNavigate();
  const location = useLocation();
  
  // Get email from navigation state (passed from Register component)
  const email = (location.state as any)?.email || '';

  useEffect(() => {
    if (!email) {
      // If no email in state, redirect to register
      navigate('/register');
    }
  }, [email, navigate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (code.length !== 6) {
      setError('Verification code must be 6 digits');
      return;
    }

    try {
      setLoading(true);
      const message = await authService.verifyEmail({ email, code });
      setSuccess(message);
      
      // Redirect to login after 2 seconds
      setTimeout(() => {
        navigate('/login', { state: { message: 'Email verified! You can now login.' } });
      }, 2000);
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Verification failed. Please check your code.';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handleResendCode = async () => {
    // TODO: Implement resend code functionality
    alert('Resend code feature will be implemented soon. Please use the code from your email.');
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <div className="verify-icon">📧</div>
        <h2 className="auth-title">Verify Your Email</h2>
        
        <p className="verify-instruction">
          We've sent a 6-digit verification code to:
        </p>
        <p className="verify-email">{email}</p>
        
        {error && (
          <div className="auth-error">
            {error}
          </div>
        )}

        {success && (
          <div className="auth-success">
            {success}
            <div className="success-redirect">Redirecting to login...</div>
          </div>
        )}
        
        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-group">
            <label htmlFor="code">Verification Code</label>
            <input
              id="code"
              type="text"
              value={code}
              onChange={(e) => setCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
              required
              maxLength={6}
              disabled={loading || !!success}
              placeholder="Enter 6-digit code"
              className="code-input"
              autoComplete="off"
              autoFocus
            />
            <small className="input-hint">Enter the code from your email</small>
          </div>

          <button type="submit" disabled={loading || !!success || code.length !== 6} className="auth-button">
            {loading ? 'Verifying...' : 'Verify Email'}
          </button>
        </form>

        <div className="verify-actions">
          <button 
            onClick={handleResendCode} 
            className="resend-button"
            disabled={loading || !!success}
          >
            Didn't receive code? Resend
          </button>
        </div>

        <p className="auth-footer">
          Wrong email? <Link to="/register">Register again</Link>
        </p>

        <div className="auth-info">
          <small>
            <strong>Note:</strong> Check your spam folder if you don't see the email.
            The code expires in 24 hours.
          </small>
        </div>
      </div>
    </div>
  );
};

export default EmailVerification;
