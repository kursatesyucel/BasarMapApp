/**
 * Generates a unique device fingerprint based on browser and system characteristics
 * This is used to identify devices for security tracking
 */
export const generateDeviceFingerprint = (): string => {
  const components = [
    navigator.userAgent,
    navigator.language,
    screen.colorDepth,
    screen.width + 'x' + screen.height,
    new Date().getTimezoneOffset(),
    !!window.sessionStorage,
    !!window.localStorage,
  ];

  // Simple hash function
  const hash = components.join('|');
  let hashValue = 0;
  
  for (let i = 0; i < hash.length; i++) {
    const char = hash.charCodeAt(i);
    hashValue = ((hashValue << 5) - hashValue) + char;
    hashValue = hashValue & hashValue; // Convert to 32bit integer
  }

  // Convert to hex and make it positive
  const fingerprint = Math.abs(hashValue).toString(16);
  
  // Store in localStorage for consistency
  const stored = localStorage.getItem('device-id');
  if (stored) {
    return stored;
  }

  localStorage.setItem('device-id', fingerprint);
  return fingerprint;
};

/**
 * Gets the stored device ID or generates a new one
 */
export const getDeviceId = (): string => {
  const stored = localStorage.getItem('device-id');
  if (stored) {
    return stored;
  }
  return generateDeviceFingerprint();
};
