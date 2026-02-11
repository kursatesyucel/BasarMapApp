import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from './hooks/useAuth';
import Login from './components/Login';
import Register from './components/Register';
import EmailVerification from './components/EmailVerification';
import ProtectedRoute from './components/ProtectedRoute';
import AdminPanel from './components/AdminPanel';
import Header from './components/Header';
import MapView from './components/MapView';
import Sidebar from './components/Sidebar';
import { useMapFeatures } from './hooks/useMapFeatures';

function App() {
  const { isAuthenticated } = useAuth();
  const mapFeatures = useMapFeatures();

  return (
    <Routes>
      {/* Public routes */}
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route path="/verify-email" element={<EmailVerification />} />

      {/* Protected routes - Main Map View */}
      <Route
        path="/"
        element={
          <ProtectedRoute>
            <div className="app-wrapper">
              <Header />
              <div className="app-container">
                <div className="map-container">
                  <MapView mapFeatures={mapFeatures} />
                </div>
                <Sidebar mapFeatures={mapFeatures} />
              </div>
            </div>
          </ProtectedRoute>
        }
      />

      {/* Admin-only route */}
      <Route
        path="/admin/users"
        element={
          <ProtectedRoute requiredRoles={['Admin']}>
            <div className="app-wrapper">
              <Header />
              <AdminPanel />
            </div>
          </ProtectedRoute>
        }
      />

      {/* Catch all - redirect to home or login */}
      <Route
        path="*"
        element={<Navigate to={isAuthenticated ? '/' : '/login'} replace />}
      />
    </Routes>
  );
}

export default App; 