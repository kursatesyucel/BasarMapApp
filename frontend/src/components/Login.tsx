import React, { useState, useEffect } from 'react';
import { useNavigate, Link, useLocation } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

const Login: React.FC = () => {
  const [loginIdentifier, setLoginIdentifier] = useState('');
  const [password, setPassword] = useState('');
  const [successMessage, setSuccessMessage] = useState('');
  const { login, loading, error, clearError, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  // Check for success message from navigation state (from email verification)
  useEffect(() => {
    const state = location.state as any;
    if (state?.message) {
      setSuccessMessage(state.message);
      // Clear the state
      window.history.replaceState({}, document.title);
    }
  }, [location]);

  // Redirect if already authenticated
  useEffect(() => {
    if (isAuthenticated) {
      // Check for redirect parameter in URL (from 401 interceptor)
      const searchParams = new URLSearchParams(location.search);
      const redirectPath = searchParams.get('redirect');
      
      if (redirectPath) {
        // Kullanıcıyı 401 öncesi bulunduğu sayfaya yönlendir
        navigate(redirectPath);
      } else {
        // Normal akış - ana sayfaya yönlendir
        navigate('/');
      }
    }
  }, [isAuthenticated, navigate, location.search]);

  // Clear messages when component unmounts
  useEffect(() => {
    return () => {
      clearError();
      setSuccessMessage('');
    };
  }, [clearError]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSuccessMessage('');
    
    const success = await login({ loginIdentifier, password });
    
    if (success) {
      // Check for redirect parameter in URL (from 401 interceptor)
      const searchParams = new URLSearchParams(location.search);
      const redirectPath = searchParams.get('redirect');
      
      if (redirectPath) {
        // Kullanıcıyı 401 öncesi bulunduğu sayfaya yönlendir
        navigate(redirectPath);
      } else {
        // Normal akış - ana sayfaya yönlendir
        navigate('/');
      }
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <h2 className="auth-title">Login to BasarMapApp</h2>
        
        {successMessage && (
          <div className="auth-success">
            {successMessage}
          </div>
        )}
        
        {error && (
          <div className="auth-error">
            {error}
          </div>
        )}
        
        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-group">
            <label htmlFor="loginIdentifier">Username or Email</label>
            <input
              id="loginIdentifier"
              type="text"
              value={loginIdentifier}
              onChange={(e) => setLoginIdentifier(e.target.value)}
              required
              disabled={loading}
              placeholder="Enter your username or email"
              autoComplete="username"
            />
            <small className="input-hint">You can login with either username or email</small>
          </div>

          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              disabled={loading}
              placeholder="Enter your password"
              autoComplete="current-password"
            />
          </div>

          <button type="submit" disabled={loading} className="auth-button">
            {loading ? 'Logging in...' : 'Login'}
          </button>
        </form>

        <p className="auth-footer">
          Don't have an account? <Link to="/register">Register here</Link>
        </p>

        <div className="auth-hint">
          <small>
            <strong>Demo credentials:</strong><br />
            Username: admin | Password: Admin123!
          </small>
        </div>
      </div>
    </div>
  );
};

export default Login;
