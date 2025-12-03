# Conexión de la Interfaz TypeScript con la Web API

Esta guía explica cómo conectar la interfaz TypeScript/React directamente con la Web API de ASP.NET Core.

## Configuración

### 1. Configurar la URL de la API

Crea un archivo `.env` en la raíz del proyecto `Tritote Nic Figma` con el siguiente contenido:

```env
VITE_API_URL=http://localhost:5000/api
```

O si usas HTTPS:

```env
VITE_API_URL=https://localhost:7000/api
```

**Nota:** Asegúrate de que la URL coincida con el puerto donde se ejecuta tu Web API. Puedes verificar el puerto en `TritoteNic/Properties/launchSettings.json`.

### 2. Iniciar la Web API

Antes de ejecutar la aplicación TypeScript, asegúrate de que la Web API esté corriendo:

```bash
cd TritoteNic
dotnet run
```

La API debería estar disponible en `http://localhost:5000` o `https://localhost:7000` (dependiendo de tu configuración).

### 3. Iniciar la aplicación TypeScript

En otra terminal:

```bash
cd "Tritote Nic Figma"
npm install
npm run dev
```

La aplicación se abrirá en `http://localhost:3000`.

## Estructura de Archivos Creados

### `src/config/api.ts`
Configuración de la URL base de la API y timeout.

### `src/types/api.ts`
Tipos TypeScript que coinciden con los DTOs de C# para mantener la consistencia entre frontend y backend.

### `src/services/apiService.ts`
Servicio singleton que maneja todas las llamadas HTTP a la API, incluyendo:
- Autenticación (login/logout)
- Gestión de tokens JWT
- CRUD de Clientes, Productos, Pedidos, Usuarios
- Dashboard y Reportes

### `src/contexts/AuthContext.tsx` (Actualizado)
Ahora maneja:
- Autenticación real con la API
- Almacenamiento de tokens JWT
- Persistencia de sesión en localStorage
- Mapeo de roles de la API a roles internos

### `src/components/Login.tsx` (Actualizado)
Ahora realiza login real contra la API en lugar de usar datos mock.

## Características Implementadas

✅ **Autenticación JWT**: Login y logout con tokens
✅ **Persistencia de sesión**: El token se guarda en localStorage
✅ **Manejo de errores**: Mensajes de error claros para el usuario
✅ **Tipos TypeScript**: Coinciden con los DTOs de C#
✅ **CORS configurado**: La Web API ya tiene CORS habilitado

## Próximos Pasos

Para completar la integración, necesitarás actualizar los componentes que actualmente usan datos mock para que usen el `apiService`:

- `Dashboard.tsx` → Usar `apiService.getDashboard()`
- `Customers.tsx` → Usar métodos de clientes del `apiService`
- `ProductsManagement.tsx` → Usar métodos de productos del `apiService`
- `OrdersManagement.tsx` → Usar métodos de pedidos del `apiService`
- `Reports.tsx` → Usar `apiService.getAnalisisCompleto()`
- `UserManagement.tsx` → Usar métodos de usuarios del `apiService`

## Solución de Problemas

### Error de CORS
Si ves errores de CORS, verifica que la Web API tenga CORS habilitado en `Program.cs` (ya está configurado con `AllowWPF` que permite cualquier origen).

### Error de conexión
- Verifica que la Web API esté corriendo
- Verifica que la URL en `.env` sea correcta
- Verifica que no haya problemas de firewall bloqueando el puerto

### Token expirado
El token JWT tiene una expiración de 60 minutos por defecto. Si expira, el usuario necesitará hacer login nuevamente.

