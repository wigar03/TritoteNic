# Instrucciones para Probar la Conexión

## Paso 1: Verificar que el archivo .env existe

El archivo `.env` debería estar en la raíz del proyecto `Tritote Nic Figma` con el siguiente contenido:

```
VITE_API_URL=http://localhost:5079/api
```

Si no existe, créalo manualmente o ejecuta en PowerShell:

```powershell
cd "Tritote Nic Figma"
"VITE_API_URL=http://localhost:5079/api" | Out-File -FilePath .env -Encoding utf8
```

## Paso 2: Iniciar la Web API

Abre una terminal y ejecuta:

```bash
cd TritoteNic
dotnet run
```

Deberías ver algo como:
```
Now listening on: http://localhost:5079
Now listening on: https://localhost:7254
```

**¡IMPORTANTE!** Deja esta terminal abierta y corriendo.

## Paso 3: Iniciar la aplicación TypeScript

Abre **otra terminal** (nueva) y ejecuta:

```bash
cd "Tritote Nic Figma"
npm install
npm run dev
```

La aplicación se abrirá automáticamente en `http://localhost:3000`

## Paso 4: Probar la conexión

1. En la pantalla de Login, verás un botón **"Probar Conexión API"**
2. Haz clic en ese botón
3. Deberías ver un mensaje de éxito si la API está corriendo
4. Si hay un error, verifica:
   - Que la Web API esté corriendo (Paso 2)
   - Que el puerto en `.env` coincida con el puerto donde corre la API
   - Que no haya errores en la consola del navegador (F12)

## Paso 5: Probar el Login

Una vez que la conexión funcione, prueba hacer login con:

- **Email:** william.garcia@tritote.com.ni
- **Contraseña:** admin123

O cualquier otro usuario que tengas en tu base de datos.

## Solución de Problemas

### Error: "No se pudo conectar a la API"
- Verifica que la Web API esté corriendo
- Verifica que el puerto en `.env` sea correcto (5079 para HTTP)
- Abre `http://localhost:5079/api/Health` en tu navegador para verificar que la API responde

### Error de CORS
- La Web API ya tiene CORS configurado, pero si ves errores, verifica que `Program.cs` tenga la política `AllowWPF` activa

### El archivo .env no se lee
- Reinicia el servidor de desarrollo (`npm run dev`)
- Vite solo lee variables de entorno que empiezan con `VITE_`
- Asegúrate de que el archivo esté en la raíz del proyecto `Tritote Nic Figma`

## Verificar la URL de la API

En la pantalla de Login, debajo del botón "Probar Conexión API", verás la URL configurada. Debería ser:
```
API: http://localhost:5079/api
```

Si es diferente, verifica tu archivo `.env`.

