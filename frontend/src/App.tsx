import React from 'react';
import { useMapFeatures } from './hooks/useMapFeatures';
import MapView from './components/MapView';
import Sidebar from './components/Sidebar';

function App() {
  const mapFeatures = useMapFeatures();

  return (
    <div className="app-container">
      <div className="map-container">
        <MapView mapFeatures={mapFeatures} />
      </div>
      <Sidebar mapFeatures={mapFeatures} />
    </div>
  );
}

export default App; 