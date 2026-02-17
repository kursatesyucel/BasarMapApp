import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from './hooks/useAuth';
import Login from './components/Login';
import Register from './components/Register';
import EmailVerification from './components/EmailVerification';
import ProtectedRoute from './components/ProtectedRoute';
import AdminPanel from './components/AdminPanel';
import LogsPage from './pages/admin/logs/LogsPage';
import Header from './components/Header';
import MapView from './components/MapView';
import MapViewWithBoundaries from './components/MapViewWithBoundaries';
import Sidebar from './components/Sidebar';
import { useMapFeatures } from './hooks/useMapFeatures';
 
function App() {
  const { isAuthenticated } = useAuth();

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
            <MapViewWrapper />
          </ProtectedRoute>
        }
      />

      {/* Admin-only routes */}
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
      <Route
        path="/admin/logs"
        element={
          <ProtectedRoute requiredRoles={['Admin']}>
            <div className="app-wrapper">
              <Header />
              <LogsPage />
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

// Separate component to avoid useMapFeatures being called on every App render
const MapViewWrapper: React.FC = () => {
  const mapFeatures = useMapFeatures();
  
  return (
    <div className="app-wrapper">
      <Header />
      <div className="app-container">
        <div className="map-container">
          <MapViewWithBoundaries mapFeatures={mapFeatures} />
        </div>
        <Sidebar mapFeatures={mapFeatures} />
      </div>
    </div>
  );
};

export default App; 