// Harita altlığı seçenekleri
export const BASEMAP_OPTIONS = [
  { id: 'standard', label: 'Standart', url: 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', attribution: '© OpenStreetMap contributors' },
  { id: 'satellite', label: 'Uydu', url: 'https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', attribution: '© Esri, Maxar, Earthstar Geographics' },
  { id: 'terrain', label: 'Arazi', url: 'https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png', attribution: '© OpenTopoMap contributors' },
  { id: 'dark', label: 'Koyu', url: 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}.png', attribution: '© CARTO' },
] as const;

export type BasemapId = typeof BASEMAP_OPTIONS[number]['id'];
