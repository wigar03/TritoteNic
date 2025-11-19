# Pendientes en la Web API - Tritote Nicaragua

## Resumen de Tareas Pendientes

### ✅ Completado
- ✅ Dashboard Controller con todos los KPIs
- ✅ Reportes Controller con análisis completo
- ✅ Mejoras en PedidoController (Include, validaciones, actualización de stock/cliente)
- ✅ Mejoras en ClienteController (Include, cálculos automáticos)
- ✅ AutoMapper actualizado
- ✅ Validaciones de stock, descuento, email único
- ✅ Documentación WPF
- ✅ Ejemplos JSON

---

## 🔴 CRÍTICO - Seguridad y Autorización

### 1. Agregar [Authorize] en Endpoints Sensibles

**Falta [Authorize] en:**
- [ ] **PedidoController** - Todos los métodos deberían requerir autenticación
  - [ ] GetPedidos()
  - [ ] GetPedido(int id)
  - [ ] PostPedido()
  - [ ] PutPedido()
  - [ ] DeletePedido()
  - [ ] PatchPedido()

- [ ] **ClienteController** - Endpoints sensibles
  - [ ] GetClientes() - Podría ser público o requerir auth
  - [ ] GetCliente(int id)
  - [ ] PostCliente() - **CRÍTICO: Requiere auth**
  - [ ] PutCliente() - **CRÍTICO: Requiere auth**
  - [ ] DeleteCliente() - **CRÍTICO: Requiere auth**
  - [ ] PatchCliente() - **CRÍTICO: Requiere auth**

- [ ] **ProductoController** - Endpoints de modificación
  - [ ] GetProductos() - Podría ser público
  - [ ] GetProducto(int id) - Podría ser público
  - [ ] PostProducto() - **CRÍTICO: Requiere auth**
  - [ ] PutProducto() - **CRÍTICO: Requiere auth**
  - [ ] DeleteProducto() - **CRÍTICO: Requiere auth**
  - [ ] PatchProducto() - **CRÍTICO: Requiere auth**

- [ ] **UsuarioController** - Todos los métodos deberían requerir auth
  - [ ] GetUsuarios()
  - [ ] GetUsuario(int id)
  - [ ] PostUsuario()
  - [ ] PutUsuario()
  - [ ] DeleteUsuario()
  - [ ] PatchUsuario()

- [ ] **CategoriaController** - Endpoints de modificación
  - [ ] PostCategoria() - **CRÍTICO: Requiere auth**
  - [ ] PutCategoria() - **CRÍTICO: Requiere auth**
  - [ ] DeleteCategoria() - **CRÍTICO: Requiere auth**
  - [ ] PatchCategoria() - **CRÍTICO: Requiere auth**

- [ ] **EstadoPedidoController** - Endpoints de modificación
  - [ ] PostEstadoPedido() - **CRÍTICO: Requiere auth**
  - [ ] PutEstadoPedido() - **CRÍTICO: Requiere auth**
  - [ ] DeleteEstadoPedido() - **CRÍTICO: Requiere auth**

- [ ] **MetodoPagoController** - Endpoints de modificación
  - [ ] PostMetodoPago() - **CRÍTICO: Requiere auth**
  - [ ] PutMetodoPago() - **CRÍTICO: Requiere auth**
  - [ ] DeleteMetodoPago() - **CRÍTICO: Requiere auth**

---

## 🟡 IMPORTANTE - Relaciones y Mapeos

### 2. Agregar .Include() para Relaciones

#### ProductoController
- [ ] **GetProductos()** - Falta `.Include(p => p.Categoria)` para mapear `NombreCategoria`
  ```csharp
  var productos = await _context.Productos
      .Include(p => p.Categoria)
      .ToListAsync();
  ```
- [ ] **GetProducto(int id)** - Falta `.Include(p => p.Categoria)`
  ```csharp
  var producto = await _context.Productos
      .Include(p => p.Categoria)
      .FirstOrDefaultAsync(p => p.IdProducto == id);
  ```
- [ ] **Mapear NombreCategoria** manualmente o usar AutoMapper (ya configurado)

#### UsuarioController
- [ ] **GetUsuarios()** - Falta `.Include(u => u.Rol)` para mapear `NombreRol`
  ```csharp
  var usuarios = await _context.Usuarios
      .Include(u => u.Rol)
      .ToListAsync();
  ```
- [ ] **GetUsuario(int id)** - Falta `.Include(u => u.Rol)`
  ```csharp
  var usuario = await _context.Usuarios
      .Include(u => u.Rol)
      .FirstOrDefaultAsync(u => u.IdUsuario == id);
  ```
- [ ] **Mapear NombreRol** manualmente o usar AutoMapper (ya configurado)

---

## 🟡 MEJORAS - Validaciones y Lógica

### 3. Validaciones Adicionales

#### ProductoController
- [ ] Validar que el precio sea positivo
- [ ] Validar que el stock no sea negativo
- [ ] Validar que la categoría exista antes de crear/actualizar
- [ ] Validar que no se elimine un producto que tiene pedidos asociados

#### CategoriaController
- [ ] Validar que no se elimine una categoría que tiene productos asociados

#### EstadoPedidoController
- [ ] Validar que no se elimine un estado que tiene pedidos asociados

#### MetodoPagoController
- [ ] Validar que no se elimine un método de pago que tiene pedidos asociados

#### RolController
- [ ] Validar que no se elimine un rol que tiene usuarios asociados

---

## 🟢 OPCIONAL - Mejoras y Optimizaciones

### 4. Paginación
- [ ] Agregar paginación en GetPedidos() si hay muchos registros
- [ ] Agregar paginación en GetClientes() si hay muchos registros
- [ ] Agregar paginación en GetProductos() si hay muchos registros
- [ ] Agregar paginación en GetUsuarios() si hay muchos registros

### 5. Filtros y Búsqueda
- [ ] Agregar filtro por categoría en GetProductos()
- [ ] Agregar búsqueda por nombre en GetProductos()
- [ ] Agregar búsqueda por nombre en GetClientes()
- [ ] Agregar filtro por estado en GetPedidos()
- [ ] Agregar filtro por fecha en GetPedidos()

### 6. Endpoints Adicionales

#### ClienteController
- [ ] GET /api/clientes/{id}/pedidos - Obtener pedidos de un cliente
- [ ] GET /api/clientes/vip - Obtener solo clientes VIP
- [ ] GET /api/clientes/por-categoria/{categoria} - Filtrar por categoría

#### ProductoController
- [ ] GET /api/productos/bajo-stock - Productos con stock bajo (< 10)
- [ ] GET /api/productos/por-categoria/{idCategoria} - Filtrar por categoría
- [ ] GET /api/productos/buscar?nombre={nombre} - Búsqueda por nombre

#### PedidoController
- [ ] GET /api/pedidos/por-fecha?fechaDesde={fecha}&fechaHasta={fecha} - Filtrar por rango de fechas
- [ ] GET /api/pedidos/por-cliente/{idCliente} - Pedidos de un cliente
- [ ] GET /api/pedidos/por-estado/{idEstado} - Pedidos por estado
- [ ] PATCH /api/pedidos/{id}/cambiar-estado - Cambiar solo el estado de un pedido

---

## 🔵 DOCUMENTACIÓN Y TESTING

### 7. Documentación Swagger
- [x] JWT configurado en Swagger ✅
- [ ] Agregar descripciones XML a los métodos de los controladores
- [ ] Agregar ejemplos de requests en Swagger
- [ ] Agregar descripciones de parámetros de query

### 8. Testing
- [ ] Crear pruebas unitarias para los controladores
- [ ] Crear pruebas de integración para los endpoints
- [ ] Probar todos los flujos de autenticación
- [ ] Probar validaciones de negocio
- [ ] Probar casos edge (valores límite, nulls, etc.)

---

## 🟣 SEGURIDAD ADICIONAL

### 9. Roles y Permisos
- [ ] Implementar sistema de roles (Administrador, Vendedor, Analista)
- [ ] Crear atributos personalizados para autorización por roles:
  ```csharp
  [Authorize(Roles = "Administrador")]
  ```
- [ ] Restringir acceso según roles:
  - Administrador: acceso total
  - Vendedor: crear pedidos, ver productos/clientes
  - Analista: solo lectura, ver reportes

### 10. Protección de Datos
- [ ] Cambiar Jwt:Key en producción (ya documentado en appsettings.json)
- [ ] Considerar usar BCrypt para contraseñas (actualmente SHA256)
- [ ] Revisar CORS para producción (actualmente AllowAnyOrigin)
- [ ] Implementar rate limiting para prevenir abuso
- [ ] Agregar logging de acciones sensibles (crear, actualizar, eliminar)

---

## 📋 PRIORIDADES

### Prioridad ALTA 🔴
1. Agregar [Authorize] en todos los endpoints sensibles
2. Agregar .Include() en ProductoController y UsuarioController
3. Mapear NombreCategoria y NombreRol correctamente

### Prioridad MEDIA 🟡
4. Validaciones adicionales en todos los controladores
5. Endpoints de filtrado y búsqueda
6. Paginación en listados grandes

### Prioridad BAJA 🟢
7. Sistema de roles y permisos
8. Mejoras de documentación Swagger
9. Testing automatizado

---

## 📝 NOTAS

- Los endpoints de lectura (GET) pueden ser públicos o requerir autenticación según la política de seguridad
- Los endpoints de modificación (POST, PUT, DELETE, PATCH) **DEBEN** requerir autenticación
- Considerar implementar versionado de API (/api/v1/) para futuras actualizaciones
- Revisar logs periódicamente para identificar problemas de seguridad

