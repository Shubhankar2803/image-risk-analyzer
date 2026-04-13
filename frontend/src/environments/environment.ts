// Development environment configuration
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5193/api',
  apiVersion: 'v1',
  
  // JWT token storage key
  tokenStorageKey: 'auth_token',
  
  // API endpoints
  endpoints: {
    auth: '/auth',
    upload: '/images/upload',
    analysis: '/images/analysis',
    history: '/images/history',
    users: '/users'
  },
  
  // File upload settings
  upload: {
    maxFileSize: 5242880, // 5MB in bytes
    allowedExtensions: ['jpg', 'jpeg', 'png', 'webp', 'gif'],
    allowedMimeTypes: ['image/jpeg', 'image/png', 'image/webp', 'image/gif']
  },
  
  // Gemini AI settings (client-side config)
  gemini: {
    riskThreshold: 50 // Risk score above this triggers warning
  }
};
