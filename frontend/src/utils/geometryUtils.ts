/**
 * Calculate the distance between two points using Haversine formula
 */
function haversineDistance(lat1: number, lon1: number, lat2: number, lon2: number): number {
  const R = 6371000; // Earth's radius in meters
  const dLat = (lat2 - lat1) * Math.PI / 180;
  const dLon = (lon2 - lon1) * Math.PI / 180;
  const a = 
    Math.sin(dLat/2) * Math.sin(dLat/2) +
    Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) * 
    Math.sin(dLon/2) * Math.sin(dLon/2);
  const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a));
  return R * c;
}

/**
 * Calculate the total length of a line from coordinates
 */
export function calculateLineLength(coordinates: number[][]): number {
  if (coordinates.length < 2) return 0;
  
  let totalLength = 0;
  for (let i = 1; i < coordinates.length; i++) {
    const [lon1, lat1] = coordinates[i - 1];
    const [lon2, lat2] = coordinates[i];
    totalLength += haversineDistance(lat1, lon1, lat2, lon2);
  }
  
  return totalLength;
}

/**
 * Calculate the area of a polygon using the shoelace formula (approximate)
 */
export function calculatePolygonArea(coordinates: number[][][]): number {
  if (coordinates.length === 0 || coordinates[0].length < 3) return 0;
  
  const ring = coordinates[0]; // Use only the exterior ring for simplicity
  let area = 0;
  
  for (let i = 0; i < ring.length - 1; i++) {
    const [x1, y1] = ring[i];
    const [x2, y2] = ring[i + 1];
    area += (x1 * y2 - x2 * y1);
  }
  
  // Convert to square meters (approximate)
  const degreeToMeter = 111320; // approximate meters per degree at equator
  return Math.abs(area * degreeToMeter * degreeToMeter / 2);
}

/**
 * Format distance for display
 */
export function formatDistance(meters: number): string {
  if (meters < 1000) {
    return `${meters.toFixed(2)} m`;
  } else {
    return `${(meters / 1000).toFixed(2)} km`;
  }
}

/**
 * Format area for display
 */
export function formatArea(squareMeters: number): string {
  if (squareMeters < 10000) {
    return `${squareMeters.toFixed(2)} m²`;
  } else {
    return `${(squareMeters / 10000).toFixed(2)} ha`;
  }
}

/**
 * Get center point of coordinates array
 */
export function getCenterPoint(coordinates: number[][]): [number, number] {
  if (coordinates.length === 0) return [0, 0];
  
  const sumLat = coordinates.reduce((sum, [lon, lat]) => sum + lat, 0);
  const sumLon = coordinates.reduce((sum, [lon, lat]) => sum + lon, 0);
  
  return [sumLon / coordinates.length, sumLat / coordinates.length];
} 