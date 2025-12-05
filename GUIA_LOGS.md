# 📋 Guía para Encontrar los Logs del Servidor y Consola

## 🔍 **1. LOGS DEL SERVIDOR (Backend - ASP.NET Core API)**

### **Opción A: Terminal/Consola donde corre la API**

Si estás ejecutando la API desde la terminal:

1. **Abre la terminal donde ejecutaste el comando:**
   ```bash
   cd "D:\Git\ProyectoFinal II Semestre\TritoteNic"
   dotnet run
   ```

2. **Los logs aparecen directamente en esa terminal.**
   - ✅ Información normal: `info:`
   - ⚠️ Advertencias: `warn:`
   - ❌ Errores: `fail:` o `Error:`

**Ejemplo de cómo se ven:**
```
info: TritoteNic.Controllers.DashboardController[0]
      Obteniendo datos del dashboard
fail: TritoteNic.Controllers.DashboardController[0]
      Error al obtener datos del dashboard: [mensaje del error]
```

---

### **Opción B: Visual Studio Output Window**

Si estás ejecutando desde Visual Studio:

1. **Abre Visual Studio**
2. **Ve al menú:** `View` → `Output` (o presiona `Ctrl+Alt+O`)
3. **En el dropdown "Show output from:"** selecciona:
   - **"Debug"** - Para ver logs en tiempo de ejecución
   - **"Web Server"** - Para ver logs específicos del servidor web

4. **Los logs aparecerán ahí mientras la aplicación corre.**

---

### **Opción C: Visual Studio Code Terminal**

Si estás usando VS Code:

1. **Abre la terminal integrada** (`Ctrl+`` o `View` → `Terminal`)
2. **Ejecuta la API:**
   ```powershell
   cd "TritoteNic"
   dotnet run
   ```
3. **Los logs aparecen directamente en la terminal.**

---

## 🌐 **2. LOGS DE LA CONSOLA DEL NAVEGADOR (Frontend)**

Para ver los logs del frontend (TypeScript/React):

1. **Abre la aplicación en tu navegador** (http://localhost:3000)

2. **Abre las Herramientas de Desarrollador:**
   - **Chrome/Edge:** Presiona `F12` o `Ctrl+Shift+I`
   - **Firefox:** Presiona `F12` o `Ctrl+Shift+K`
   - **Safari:** `Cmd+Option+I` (Mac)

3. **Ve a la pestaña "Console":**
   - Verás todos los `console.log()`, errores de JavaScript, y errores de red aquí.

4. **También puedes revisar la pestaña "Network":**
   - Aquí verás todas las peticiones HTTP
   - Filtra por "XHR" o "Fetch" para ver solo las llamadas a la API
   - Haz clic en una petición fallida para ver detalles:
     - **Status:** 500, 404, etc.
     - **Response:** El mensaje de error del servidor

---

## 🐛 **Cómo Capturar el Error Específico**

### **Para errores del servidor (500):**

1. **Asegúrate de que la API esté corriendo**
2. **Intenta acceder al Dashboard o Reportes desde el navegador**
3. **Inmediatamente revisa los logs del servidor** (Opción A, B o C arriba)
4. **Busca líneas que contengan:**
   - `Error al obtener datos del dashboard`
   - `Error al obtener análisis completo`
   - `Stack trace:`
   - `Inner exception:`

### **Para errores del frontend:**

1. **Abre la consola del navegador** (F12 → Console)
2. **Intenta cargar el Dashboard o Reportes**
3. **Busca mensajes en rojo** que indiquen errores
4. **También revisa la pestaña Network** para ver si hay peticiones fallidas

---

## 📸 **Ejemplo de Logs del Servidor**

Cuando hay un error, verás algo así en la terminal del servidor:

```
info: TritoteNic.Controllers.DashboardController[0]
      Obteniendo datos del dashboard
fail: TritoteNic.Controllers.DashboardController[0]
      Error al obtener datos del dashboard: Object reference not set to an instance of an object.
fail: System.Exception[0]
      Stack trace: 
         at TritoteNic.Controllers.DashboardController.GetDashboard() in D:\Git\...\DashboardController.cs:line 200
         at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor...
fail: System.Exception[0]
      Inner exception: System.NullReferenceException: Object reference not set to an instance of an object.
```

**Copia todo este mensaje de error para diagnosticar el problema.**

---

## 🎯 **Pasos Recomendados para Diagnosticar tu Problema**

1. ✅ **Abre la terminal donde corre la API** (o Visual Studio Output)
2. ✅ **Abre el navegador** y ve a http://localhost:3000
3. ✅ **Abre la consola del navegador** (F12 → Console)
4. ✅ **Intenta cargar el Dashboard**
5. ✅ **Observa ambos lugares simultáneamente:**
   - Terminal del servidor: Verás el error del backend
   - Consola del navegador: Verás cómo el frontend recibe el error

6. ✅ **Copia el mensaje de error completo** de la terminal del servidor

---

## 💡 **Tip Extra: Logs en Archivo**

Si quieres guardar los logs en un archivo, puedes modificar `Program.cs` para agregar logging a archivo. Por ahora, los logs solo van a la consola.

