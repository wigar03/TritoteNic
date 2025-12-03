// Utilidad para probar la conexión con la API
import { API_CONFIG } from '../config/api';

export async function testApiConnection(): Promise<{ success: boolean; message: string }> {
  try {
    const response = await fetch(`${API_CONFIG.baseURL.replace('/api', '')}/api/Health`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      return {
        success: false,
        message: `Error: ${response.status} ${response.statusText}`,
      };
    }

    const data = await response.json();
    return {
      success: true,
      message: `Conexión exitosa. Base de datos: ${data.database || 'N/A'}`,
    };
  } catch (error) {
    const errorMessage = error instanceof Error ? error.message : 'Error desconocido';
    return {
      success: false,
      message: `No se pudo conectar a la API: ${errorMessage}. Verifica que la Web API esté corriendo en ${API_CONFIG.baseURL}`,
    };
  }
}

