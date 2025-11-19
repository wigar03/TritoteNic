# Guía de Integración WPF - Tritote Nicaragua API

## Índice
1. [Configuración Inicial](#configuración-inicial)
2. [Autenticación JWT](#autenticación-jwt)
3. [Cliente HTTP](#cliente-http)
4. [Endpoints y Ejemplos](#endpoints-y-ejemplos)
5. [Manejo de Errores](#manejo-de-errores)
6. [Estructura de Respuestas](#estructura-de-respuestas)

---

## Configuración Inicial

### URL Base de la API
```csharp
private const string BaseUrl = "https://localhost:7000/api"; // Desarrollo
// En producción: "https://api.tritote.com.ni/api"
```

### Dependencias NuGet Requeridas
- `System.Net.Http.Json` (incluido en .NET 9)
- `Newtonsoft.Json` (opcional, si necesitas serialización personalizada)

---

## Autenticación JWT

### Flujo de Autenticación

1. **Login inicial**
```csharp
POST /api/auth/login
Content-Type: application/json

{
    "email": "admin@tritote.com.ni",
    "password": "admin123"
}
```

2. **Respuesta exitosa**
```json
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "usuario": {
        "idUsuario": 1,
        "nombreUsuario": "Laura Administradora",
        "emailUsuario": "admin@tritote.com.ni",
        "estadoUsuario": "Activo",
        "fechaCreacionUsuario": "2025-01-01T00:00:00",
        "ultimoAcceso": "2025-11-10T14:30:00",
        "idRol": 1,
        "nombreRol": "Administrador"
    },
    "expiresAt": "2025-11-10T15:30:00"
}
```

3. **Usar token en requests**
```csharp
httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", token);
```

### Ejemplo de Clase de Autenticación

```csharp
public class AuthService
{
    private readonly HttpClient _httpClient;
    private string? _token;

    public AuthService(string baseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<LoginResponseDto?> LoginAsync(string email, string password)
    {
        var loginDto = new LoginDto
        {
            Email = email,
            Password = password
        };

        var response = await _httpClient.PostAsJsonAsync("/auth/login", loginDto);
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            _token = result?.Token;
            
            if (!string.IsNullOrEmpty(_token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", _token);
            }
            
            return result;
        }
        
        throw new HttpRequestException($"Login fallido: {response.StatusCode}");
    }

    public void SetToken(string? token)
    {
        _token = token;
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
```

---

## Cliente HTTP

### Cliente HTTP Básico

```csharp
public class ApiService
{
    private readonly HttpClient _httpClient;
    private string _baseUrl;

    public ApiService(string baseUrl = "https://localhost:7000/api")
    {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl)
        };
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // GET genérico
    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    // POST genérico
    public async Task<T?> PostAsync<T>(string endpoint, object? data = null)
    {
        var content = data != null 
            ? JsonContent.Create(data) 
            : null;

        var response = await _httpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    // PUT genérico
    public async Task<T?> PutAsync<T>(string endpoint, object data)
    {
        var content = JsonContent.Create(data);
        var response = await _httpClient.PutAsync(endpoint, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    // DELETE genérico
    public async Task<bool> DeleteAsync(string endpoint)
    {
        var response = await _httpClient.DeleteAsync(endpoint);
        return response.IsSuccessStatusCode;
    }
}
```

---

## Endpoints y Ejemplos

### 1. Dashboard

**GET** `/api/dashboard`  
**Autorización:** Requerida (Bearer Token)

```csharp
var dashboard = await apiService.GetAsync<DashboardDto>("/dashboard");
```

**Respuesta:**
```json
{
    "ventasKpi": {
        "ventasDia": 8945.00,
        "ventasSemana": 48300.00,
        "ventasMes": 125400.00,
        "porcentajeCambioDia": 12.5,
        "porcentajeCambioSemana": 8.2,
        "porcentajeCambioMes": 15.3
    },
    "pedidosKpi": {
        "pedidosActivos": 32,
        "pedidosPendientes": 18,
        "pedidosEnProceso": 14,
        "totalPedidos": 32
    },
    "alertas": [
        {
            "tipo": "StockBajo",
            "mensaje": "Producto Tote Bag Beige tiene stock bajo: 8 unidades",
            "idProducto": 3,
            "nombreProducto": "Tote Bag Beige",
            "stockActual": 8,
            "idPedido": null,
            "diasRetraso": null
        },
        {
            "tipo": "PedidoRetrasado",
            "mensaje": "Pedido #7 tiene 5 días de retraso",
            "idProducto": null,
            "nombreProducto": null,
            "stockActual": null,
            "idPedido": 7,
            "diasRetraso": 5
        }
    ],
    "ventasDiarias": [
        {
            "fecha": "2025-11-04T00:00:00",
            "totalVentas": 5200.00,
            "cantidadPedidos": 3
        },
        {
            "fecha": "2025-11-05T00:00:00",
            "totalVentas": 6700.00,
            "cantidadPedidos": 4
        }
        // ... más días
    ],
    "productosMasVendidos": [
        {
            "idProducto": 1,
            "nombreProducto": "Bolso Clásico Negro",
            "cantidadVendida": 145,
            "totalVentas": 1232500.00
        }
        // ... más productos
    ]
}
```

### 2. Reportes

**GET** `/api/reportes/analisis-completo?mes=11&año=2025`  
**Autorización:** Requerida

```csharp
var analisis = await apiService.GetAsync<AnalisisCompletoDto>(
    "/reportes/analisis-completo?mes=11&año=2025");
```

**GET** `/api/reportes/tendencias-color?fechaDesde=2025-05-01&fechaHasta=2025-11-10`  
**Autorización:** Requerida

```csharp
var tendencias = await apiService.GetAsync<TendenciasColorDto>(
    "/reportes/tendencias-color?fechaDesde=2025-05-01&fechaHasta=2025-11-10");
```

### 3. Pedidos

**GET** `/api/pedidos` - Listar todos los pedidos

```csharp
var pedidos = await apiService.GetAsync<List<PedidoDto>>("/pedidos");
```

**GET** `/api/pedidos/{id}` - Obtener un pedido

```csharp
var pedido = await apiService.GetAsync<PedidoDto>("/pedidos/1");
```

**POST** `/api/pedidos` - Crear un pedido

```csharp
var nuevoPedido = new PedidoCreateDto
{
    IdCliente = 1,
    IdUsuario = 1,
    IdEstadoPedido = 1,
    IdMetodoPago = 1,
    SubtotalPedido = 9200.00,
    Descuento = 5, // 5%
    TotalPedido = 8740.00,
    Detalles = new List<DetallePedidoCreateDto>
    {
        new DetallePedidoCreateDto
        {
            IdProducto = 4,
            CantidadProducto = 1,
            PrecioUnitarioProducto = 9200.00,
            SubtotalProducto = 9200.00
        }
    }
};

var pedidoCreado = await apiService.PostAsync<PedidoDto>("/pedidos", nuevoPedido);
```

**Respuesta del POST:**
```json
{
    "idPedido": 8,
    "idCliente": 1,
    "nombreCliente": "María González",
    "idUsuario": 1,
    "nombreUsuario": "Laura Administradora",
    "idEstadoPedido": 1,
    "nombreEstadoPedido": "Pendiente",
    "idMetodoPago": 1,
    "nombreMetodoPago": "Transferencia",
    "fechaPedido": "2025-11-10T14:30:00",
    "subtotalPedido": 9200.00,
    "descuento": 5.00,
    "totalPedido": 8740.00,
    "detalles": [
        {
            "idDetalle": 15,
            "idPedido": 8,
            "idProducto": 4,
            "nombreProducto": "Mochila Urbana",
            "cantidadProducto": 1,
            "precioUnitarioProducto": 9200.00,
            "subtotalProducto": 9200.00
        }
    ]
}
```

### 4. Clientes

**GET** `/api/clientes` - Listar todos los clientes

```csharp
var clientes = await apiService.GetAsync<List<ClienteDto>>("/clientes");
```

**Respuesta:**
```json
[
    {
        "idCliente": 1,
        "nombreCliente": "María González",
        "telefonoCliente": "+54 9 11 2345-6789",
        "direccionCliente": "Calle Principal 123",
        "emailCliente": "maria.g@email.com",
        "categoriaCliente": "Frecuente",
        "totalGastado": 127500.00,
        "fechaUltimoPedido": "2025-10-05T10:30:00",
        "totalPedidos": 15
    }
]
```

### 5. Productos

**GET** `/api/productos` - Listar todos los productos

```csharp
var productos = await apiService.GetAsync<List<ProductoDto>>("/productos");
```

---

## Manejo de Errores

### Códigos HTTP Comunes

- **200 OK**: Operación exitosa
- **201 Created**: Recurso creado exitosamente
- **400 Bad Request**: Error de validación o datos incorrectos
- **401 Unauthorized**: Token inválido o expirado
- **404 Not Found**: Recurso no encontrado
- **500 Internal Server Error**: Error del servidor

### Ejemplo de Manejo de Errores

```csharp
public async Task<T?> GetWithErrorHandling<T>(string endpoint)
{
    try
    {
        var response = await _httpClient.GetAsync(endpoint);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }
        
        // Manejar diferentes códigos de error
        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                // Token expirado - redirigir al login
                OnTokenExpired?.Invoke();
                throw new UnauthorizedAccessException("Sesión expirada");
                
            case HttpStatusCode.BadRequest:
                // Error de validación
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new ValidationException(errorContent);
                
            case HttpStatusCode.NotFound:
                throw new NotFoundException("Recurso no encontrado");
                
            default:
                response.EnsureSuccessStatusCode();
                return default;
        }
    }
    catch (HttpRequestException ex)
    {
        // Error de red
        throw new ApiException("Error al conectar con el servidor", ex);
    }
}
```

### Respuesta de Error Estándar

Cuando hay errores de validación (400 Bad Request):

```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "EmailCliente": [
            "El email ya está en uso."
        ],
        "Descuento": [
            "El descuento debe estar entre 0 y 100."
        ]
    },
    "traceId": "00-..."
}
```

---

## Estructura de Respuestas

### LoginResponseDto
```csharp
public class LoginResponseDto
{
    public string Token { get; set; }
    public UsuarioDto Usuario { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

### DashboardDto
```csharp
public class DashboardDto
{
    public VentasKpiDto? VentasKpi { get; set; }
    public PedidosKpiDto? PedidosKpi { get; set; }
    public List<AlertaDto>? Alertas { get; set; }
    public List<VentasDiariasDto>? VentasDiarias { get; set; }
    public List<ProductoVendidoDto>? ProductosMasVendidos { get; set; }
}
```

### PedidoDto
```csharp
public class PedidoDto
{
    public int IdPedido { get; set; }
    public int IdCliente { get; set; }
    public string? NombreCliente { get; set; }
    public int IdUsuario { get; set; }
    public string? NombreUsuario { get; set; }
    public int IdEstadoPedido { get; set; }
    public string? NombreEstadoPedido { get; set; }
    public int IdMetodoPago { get; set; }
    public string? NombreMetodoPago { get; set; }
    public DateTime FechaPedido { get; set; }
    public decimal? SubtotalPedido { get; set; }
    public decimal Descuento { get; set; }
    public decimal TotalPedido { get; set; }
    public List<DetallePedidoDto>? Detalles { get; set; }
}
```

---

## Notas Importantes

1. **Token JWT**: Los tokens tienen un tiempo de expiración. Implementa lógica para refrescar el token antes de que expire.

2. **CORS**: La API está configurada para aceptar requests desde cualquier origen en desarrollo. En producción, ajusta la política CORS.

3. **Validaciones del Cliente**:
   - El `TotalGastado`, `TotalPedidos` y `FechaUltimoPedido` se calculan automáticamente desde los pedidos.
   - La `CategoriaCliente` se actualiza automáticamente según el total gastado:
     - VIP: >= $100,000
     - Frecuente: >= $50,000
     - Regular: > $0

4. **Validaciones del Pedido**:
   - Se valida el stock antes de crear el pedido.
   - Se actualiza automáticamente el stock de productos.
   - Se valida que el descuento esté entre 0-100%.
   - Si no se proporciona `SubtotalPedido`, se calcula desde los detalles.
   - Si no se proporciona `TotalPedido`, se calcula aplicando el descuento.

5. **Email Único**: Tanto en clientes como usuarios, el email debe ser único. Si intentas crear uno duplicado, recibirás un error 400.

---

## Ejemplo Completo de Uso

```csharp
// 1. Inicializar servicios
var authService = new AuthService("https://localhost:7000/api");
var apiService = new ApiService("https://localhost:7000/api");

// 2. Login
var loginResult = await authService.LoginAsync(
    "admin@tritote.com.ni", 
    "admin123");

if (loginResult != null)
{
    // 3. Configurar token
    apiService.SetToken(loginResult.Token);
    
    // 4. Obtener dashboard
    var dashboard = await apiService.GetAsync<DashboardDto>("/dashboard");
    
    // 5. Crear pedido
    var nuevoPedido = new PedidoCreateDto { /* ... */ };
    var pedidoCreado = await apiService.PostAsync<PedidoDto>("/pedidos", nuevoPedido);
}
```

