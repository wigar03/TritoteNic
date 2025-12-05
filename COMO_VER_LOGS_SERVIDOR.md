# 🔍 Cómo Ver los Logs del Servidor (Backend)

Los logs que viste en la consola del navegador son del **frontend**. Necesitamos ver los logs del **servidor** (la API que corre en `localhost:5079`).

## 📍 **Ubicación de los Logs del Servidor:**

### **Opción 1: Terminal donde ejecutaste `dotnet run`**

1. **Busca la terminal/PowerShell** donde ejecutaste:
   ```powershell
   cd "TritoteNic"
   dotnet run
   ```

2. **Ahí aparecerán los logs en tiempo real**, algo como:
   ```
   info: TritoteNic.Controllers.DashboardController[0]
         Obteniendo datos del dashboard
   fail: TritoteNic.Controllers.DashboardController[0]
         Error al obtener datos del dashboard: [mensaje específico]
   ```

### **Opción 2: Ver el mensaje de error en el Navegador**

1. **Abre el navegador** y presiona `F12` (Herramientas de Desarrollador)
2. **Ve a la pestaña "Network"**
3. **Intenta cargar el Dashboard** (o recarga la página)
4. **Busca la petición** a `http://localhost:5079/api/Dashboard` que tenga status **500**
5. **Haz clic en esa petición**
6. **Ve a la pestaña "Response"** o **"Preview"**
7. **Ahí verás el mensaje JSON de error** que devuelve el servidor

   Ejemplo:
   ```json
   {
     "message": "Error interno del servidor al obtener los datos del dashboard.",
     "error": "Object reference not set to an instance of an object."
   }
   ```

### **Opción 3: Visual Studio Output Window**

Si estás usando Visual Studio:

1. **Abre Visual Studio**
2. **Ve a:** `View` → `Output` (o presiona `Ctrl+Alt+O`)
3. **En el dropdown "Show output from:"** selecciona **"Debug"**
4. **Ahí verás los logs del servidor**

---

## 🎯 **Lo que Necesito para Diagnosticar:**

1. ✅ **El mensaje de error específico** del servidor (de cualquiera de las opciones arriba)
2. ✅ **O una captura de pantalla** de la pestaña "Response" en Network del navegador

Con ese mensaje podré identificar exactamente qué está fallando.

