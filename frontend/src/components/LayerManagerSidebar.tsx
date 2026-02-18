import React, { useState, useMemo } from 'react';
import { 
  Layers, 
  MapPin, 
  Circle, 
  Navigation, 
  ChevronDown, 
  ChevronRight,
  ChevronLeft,
  Search,
  Map
} from 'lucide-react';
import { SettlementDto } from '../services/boundaryService';
import { BASEMAP_OPTIONS, BasemapId } from '../constants/basemaps';

// ============= Types =============

export interface LayerState {
  provinceBoundaries: boolean;
  cityMarkers: boolean; // BAŞKENT + İL
  districtMarkers: boolean; // İLÇE
}

export interface LayerOpacity {
  provinceBoundaries: number;
  cityMarkers: number;
  districtMarkers: number;
}

interface LayerManagerSidebarProps {
  layerState: LayerState;
  onLayerToggle: (layerKey: keyof LayerState) => void;
  layerOpacity: LayerOpacity;
  onOpacityChange: (layerKey: keyof LayerOpacity, value: number) => void;
  cityAndCapitalCenters: SettlementDto[];
  onCitySelect: (settlement: SettlementDto) => void;
  isLoading?: boolean;
  basemap: BasemapId;
  onBasemapChange: (basemap: BasemapId) => void;
}

const LayerManagerSidebar: React.FC<LayerManagerSidebarProps> = ({
  layerState,
  onLayerToggle,
  layerOpacity,
  onOpacityChange,
  cityAndCapitalCenters,
  onCitySelect,
  isLoading = false,
  basemap,
  onBasemapChange
}) => {
  const [isCollapsed, setIsCollapsed] = useState(false);
  const [isBoundariesExpanded, setIsBoundariesExpanded] = useState(true);
  const [isSettlementsExpanded, setIsSettlementsExpanded] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');

  // Filtreli şehir listesi
  const filteredCities = useMemo(() => {
    if (!searchQuery.trim()) return cityAndCapitalCenters;
    
    const query = searchQuery.toLowerCase().trim();
    return cityAndCapitalCenters.filter(city => 
      city.name.toLowerCase().includes(query)
    );
  }, [cityAndCapitalCenters, searchQuery]);

  return (
    <div className={`layer-manager-sidebar ${isCollapsed ? 'layer-manager-sidebar--collapsed' : ''}`}>
      {/* Header */}
      <div className="sidebar-header-glass layer-manager-header">
        <div className="flex items-center gap-2 flex-1 min-w-0">
          <Layers className="w-5 h-5 text-blue-400 flex-shrink-0" />
          {!isCollapsed && <h2 className="text-lg font-bold text-white truncate">Katman Yöneticisi</h2>}
        </div>
        <button
          type="button"
          className="layer-manager-toggle-btn"
          onClick={(e) => {
            e.stopPropagation();
            setIsCollapsed(!isCollapsed);
          }}
          title={isCollapsed ? 'Katman Yöneticisini Aç' : 'Katman Yöneticisini Kapat'}
          aria-label={isCollapsed ? 'Katman Yöneticisini Aç' : 'Katman Yöneticisini Kapat'}
        >
          {isCollapsed ? (
            <ChevronRight className="w-5 h-5" />
          ) : (
            <ChevronLeft className="w-5 h-5" />
          )}
        </button>
      </div>

      {/* Content */}
      {!isCollapsed && (
      <div className="sidebar-content-scroll">
        {/* Harita Altlığı */}
        <div className="layer-section">
          <div className="section-header-glass">
            <Map className="w-4 h-4 text-blue-400" />
            <span className="font-semibold text-white">Harita Altlığı</span>
          </div>
          <div className="p-3">
            <select
              className="city-select-glass basemap-select w-full"
              value={basemap}
              onChange={(e) => onBasemapChange(e.target.value as BasemapId)}
            >
              {BASEMAP_OPTIONS.map((opt) => (
                <option key={opt.id} value={opt.id}>
                  {opt.label}
                </option>
              ))}
            </select>
          </div>
        </div>

        {/* Navigasyon - Şehir Seçimi */}
        <div className="layer-section">
          <div className="section-header-glass">
            <Navigation className="w-4 h-4 text-blue-400" />
            <span className="font-semibold text-white">Şehir Navigasyonu</span>
          </div>
          
          <div className="p-3 space-y-2">
            {/* Arama kutusu */}
            <div className="search-box-glass">
              <Search className="w-4 h-4 text-gray-400" />
              <input
                type="text"
                placeholder="Şehir ara..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="search-input-glass"
              />
            </div>

            {/* Şehir listesi */}
            <div className="city-select-container">
              {isLoading ? (
                <div className="text-center text-gray-400 text-sm py-4">
                  Yükleniyor...
                </div>
              ) : filteredCities.length === 0 ? (
                <div className="text-center text-gray-400 text-sm py-4">
                  {searchQuery ? 'Sonuç bulunamadı' : 'Şehir verisi yok'}
                </div>
              ) : (
                <select
                  className="city-select-glass"
                  onChange={(e) => {
                    const settlement = cityAndCapitalCenters.find(
                      s => s.id === parseInt(e.target.value)
                    );
                    if (settlement) onCitySelect(settlement);
                  }}
                  defaultValue=""
                >
                  <option value="" disabled>Şehir seçin...</option>
                  {filteredCities.map(city => (
                    <option key={city.id} value={city.id}>
                      {city.category === 'BAŞKENT' ? '⭐ ' : '📍 '}{city.name}
                    </option>
                  ))}
                </select>
              )}
            </div>
          </div>
        </div>

        {/* Sınır Verileri Grubu */}
        <div className="layer-section">
          <button
            className="section-header-glass w-full"
            onClick={() => setIsBoundariesExpanded(!isBoundariesExpanded)}
          >
            {isBoundariesExpanded ? (
              <ChevronDown className="w-4 h-4 text-blue-400" />
            ) : (
              <ChevronRight className="w-4 h-4 text-blue-400" />
            )}
            <span className="font-semibold text-white">Sınır Verileri</span>
          </button>

          {isBoundariesExpanded && (
            <div className="p-3 space-y-3">
              {/* İl Sınırları */}
              <div className="layer-item-glass">
                <div className="flex items-center justify-between mb-2">
                  <label className="flex items-center gap-2 cursor-pointer flex-1">
                    <input
                      type="checkbox"
                      checked={layerState.provinceBoundaries}
                      onChange={() => onLayerToggle('provinceBoundaries')}
                      className="layer-checkbox"
                    />
                    <Circle className="w-4 h-4 text-purple-400" />
                    <span className="text-white text-sm font-medium">İl Sınırları</span>
                  </label>
                </div>
                
                {layerState.provinceBoundaries && (
                  <div className="opacity-slider-container">
                    <label className="text-xs text-gray-300">Şeffaflık</label>
                    <input
                      type="range"
                      min="0"
                      max="100"
                      value={layerOpacity.provinceBoundaries * 100}
                      onChange={(e) => onOpacityChange('provinceBoundaries', parseInt(e.target.value) / 100)}
                      className="opacity-slider"
                    />
                    <span className="text-xs text-gray-400">
                      {Math.round(layerOpacity.provinceBoundaries * 100)}%
                    </span>
                  </div>
                )}
              </div>
            </div>
          )}
        </div>

        {/* Yerleşim Yerleri Grubu */}
        <div className="layer-section">
          <button
            className="section-header-glass w-full"
            onClick={() => setIsSettlementsExpanded(!isSettlementsExpanded)}
          >
            {isSettlementsExpanded ? (
              <ChevronDown className="w-4 h-4 text-blue-400" />
            ) : (
              <ChevronRight className="w-4 h-4 text-blue-400" />
            )}
            <span className="font-semibold text-white">Yerleşim Yerleri</span>
          </button>

          {isSettlementsExpanded && (
            <div className="p-3 space-y-3">
              {/* Şehir Merkezleri (BAŞKENT + İL) */}
              <div className="layer-item-glass">
                <div className="flex items-center justify-between mb-2">
                  <label className="flex items-center gap-2 cursor-pointer flex-1">
                    <input
                      type="checkbox"
                      checked={layerState.cityMarkers}
                      onChange={() => onLayerToggle('cityMarkers')}
                      className="layer-checkbox"
                    />
                    <MapPin className="w-4 h-4 text-red-400" />
                    <span className="text-white text-sm font-medium">Şehir Merkezleri</span>
                  </label>
                  <span className="text-xs text-gray-400">BAŞKENT + İL</span>
                </div>
                
                {layerState.cityMarkers && (
                  <div className="opacity-slider-container">
                    <label className="text-xs text-gray-300">Şeffaflık</label>
                    <input
                      type="range"
                      min="0"
                      max="100"
                      value={layerOpacity.cityMarkers * 100}
                      onChange={(e) => onOpacityChange('cityMarkers', parseInt(e.target.value) / 100)}
                      className="opacity-slider"
                    />
                    <span className="text-xs text-gray-400">
                      {Math.round(layerOpacity.cityMarkers * 100)}%
                    </span>
                  </div>
                )}
              </div>

              {/* İlçe Merkezleri */}
              <div className="layer-item-glass">
                <div className="flex items-center justify-between mb-2">
                  <label className="flex items-center gap-2 cursor-pointer flex-1">
                    <input
                      type="checkbox"
                      checked={layerState.districtMarkers}
                      onChange={() => onLayerToggle('districtMarkers')}
                      className="layer-checkbox"
                    />
                    <Circle className="w-3 h-3 text-orange-400" />
                    <span className="text-white text-sm font-medium">İlçe Merkezleri</span>
                  </label>
                  <span className="text-xs text-gray-400">İLÇE</span>
                </div>
                
                {layerState.districtMarkers && (
                  <div className="opacity-slider-container">
                    <label className="text-xs text-gray-300">Şeffaflık</label>
                    <input
                      type="range"
                      min="0"
                      max="100"
                      value={layerOpacity.districtMarkers * 100}
                      onChange={(e) => onOpacityChange('districtMarkers', parseInt(e.target.value) / 100)}
                      className="opacity-slider"
                    />
                    <span className="text-xs text-gray-400">
                      {Math.round(layerOpacity.districtMarkers * 100)}%
                    </span>
                  </div>
                )}
              </div>
            </div>
          )}
        </div>

        {/* Bilgi Kutusu */}
        <div className="info-box-glass">
          <div className="text-xs text-gray-300 leading-relaxed">
            <strong className="text-white">İpucu:</strong> Katmanları harita üzerinde görmek için 
            checkbox'ları işaretleyin. Şeffaflık seviyesini ayarlamak için slider kullanın.
          </div>
        </div>
      </div>
      )}
    </div>
  );
};

export default LayerManagerSidebar;
