// Configuración de la API
export const API_CONFIG = {
  // Cambiar según el entorno donde se ejecute la Web API
  // Por defecto usa el puerto 5079 (HTTP) según launchSettings.json
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5079/api',
  // Para desarrollo local, usar: 'http://localhost:5079/api' o 'https://localhost:7254/api'
  timeout: 30000, // 30 segundos
};

