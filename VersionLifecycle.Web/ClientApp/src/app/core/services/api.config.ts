// Auto-select API base URL based on runtime origin.
// - Dev (Angular CLI on 4200): point to backend on 5000
// - Docker/Prod (served by nginx on 80): use same-origin relative '/api'
const detectApiUrl = () => {
  try {
    const loc = window?.location;
    if (!loc) return '/api';
    if (loc.port === '4200') return 'http://localhost:5000/api';
    return '/api';
  } catch {
    return '/api';
  }
};

export const API_CONFIG = {
  apiUrl: detectApiUrl()
};
