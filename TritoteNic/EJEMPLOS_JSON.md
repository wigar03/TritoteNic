# Ejemplos de Respuestas JSON - Tritote Nicaragua API

Este documento contiene ejemplos de respuestas JSON de todos los endpoints principales de la API.

---

## Autenticación

### POST /api/auth/login

**Request:**
```json
{
    "email": "admin@tritote.com.ni",
    "password": "admin123"
}
```

**Response 200 OK:**
```json
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOjEsImVtYWlsIjoiYWRtaW5AdHJpdG90ZS5jb20ubmkiLCJyb2wiOiJBZG1pbmlzdHJhZG9yIiwiaWF0IjoxNzI4OTgxODAwLCJleHAiOjE3MjkwMTc4MDB9.signature",
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

**Response 401 Unauthorized:**
```json
{
    "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
    "title": "Unauthorized",
    "status": 401,
    "detail": "Credenciales inválidas."
}
```

---

## Dashboard

### GET /api/dashboard

**Response 200 OK:**
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
            "tipo": "StockBajo",
            "mensaje": "Producto Mochila Urbana tiene stock bajo: 5 unidades",
            "idProducto": 4,
            "nombreProducto": "Mochila Urbana",
            "stockActual": 5,
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
        },
        {
            "fecha": "2025-11-06T00:00:00",
            "totalVentas": 8500.00,
            "cantidadPedidos": 5
        },
        {
            "fecha": "2025-11-07T00:00:00",
            "totalVentas": 6200.00,
            "cantidadPedidos": 3
        },
        {
            "fecha": "2025-11-08T00:00:00",
            "totalVentas": 9400.00,
            "cantidadPedidos": 6
        },
        {
            "fecha": "2025-11-09T00:00:00",
            "totalVentas": 7800.00,
            "cantidadPedidos": 4
        },
        {
            "fecha": "2025-11-10T00:00:00",
            "totalVentas": 8945.00,
            "cantidadPedidos": 7
        }
    ],
    "productosMasVendidos": [
        {
            "idProducto": 1,
            "nombreProducto": "Bolso Clásico Negro",
            "cantidadVendida": 145,
            "totalVentas": 1232500.00
        },
        {
            "idProducto": 2,
            "nombreProducto": "Cartera Mini Rosa",
            "cantidadVendida": 132,
            "totalVentas": 594000.00
        },
        {
            "idProducto": 3,
            "nombreProducto": "Tote Bag Beige",
            "cantidadVendida": 118,
            "totalVentas": 790600.00
        },
        {
            "idProducto": 4,
            "nombreProducto": "Mochila Urbana",
            "cantidadVendida": 98,
            "totalVentas": 901600.00
        },
        {
            "idProducto": 5,
            "nombreProducto": "Clutch Dorado",
            "cantidadVendida": 87,
            "totalVentas": 461100.00
        }
    ]
}
```

---

## Reportes

### GET /api/reportes/analisis-completo?mes=11&año=2025

**Response 200 OK:**
```json
{
    "comparativaVentas": {
        "ventasSemanales": [
            {
                "periodo": "Semana 1",
                "ventasActuales": 38000.00,
                "ventasAnteriores": 34000.00,
                "porcentajeCambio": 11.76,
                "fechaInicio": "2025-11-01T00:00:00",
                "fechaFin": "2025-11-07T00:00:00"
            },
            {
                "periodo": "Semana 2",
                "ventasActuales": 47000.00,
                "ventasAnteriores": 39000.00,
                "porcentajeCambio": 20.51,
                "fechaInicio": "2025-11-08T00:00:00",
                "fechaFin": "2025-11-14T00:00:00"
            }
        ],
        "totalPeriodoActual": 125400.00,
        "totalPeriodoAnterior": 108700.00,
        "porcentajeCambio": 15.36
    },
    "tendenciasColor": [
        {
            "color": "Negro",
            "cantidadVendida": 245,
            "totalVentas": 2082500.00,
            "porcentajeVentas": 32.5,
            "cantidadProductos": 15
        },
        {
            "color": "Beige",
            "cantidadVendida": 180,
            "totalVentas": 1206000.00,
            "porcentajeVentas": 18.8,
            "cantidadProductos": 12
        },
        {
            "color": "Rosa",
            "cantidadVendida": 165,
            "totalVentas": 742500.00,
            "porcentajeVentas": 11.6,
            "cantidadProductos": 8
        },
        {
            "color": "Dorado",
            "cantidadVendida": 142,
            "totalVentas": 752600.00,
            "porcentajeVentas": 11.7,
            "cantidadProductos": 10
        }
    ],
    "tendenciasTemporada": [
        {
            "mes": 1,
            "nombreMes": "enero",
            "totalVentas": 85000.00,
            "cantidadPedidos": 45,
            "promedioVenta": 1888.89
        },
        {
            "mes": 2,
            "nombreMes": "febrero",
            "totalVentas": 92000.00,
            "cantidadPedidos": 52,
            "promedioVenta": 1769.23
        }
    ],
    "productosRotacion": [
        {
            "idProducto": 1,
            "nombreProducto": "Bolso Clásico Negro",
            "categoria": "Bolsos",
            "cantidadVendida": 145,
            "totalVentas": 1232500.00,
            "rotacion": 14.5,
            "tipoRotacion": "Alta"
        },
        {
            "idProducto": 10,
            "nombreProducto": "Bolso XXL",
            "categoria": "Bolsos",
            "cantidadVendida": 8,
            "totalVentas": 68000.00,
            "rotacion": 0.16,
            "tipoRotacion": "Baja"
        }
    ]
}
```

---

## Pedidos

### GET /api/pedidos

**Response 200 OK:**
```json
[
    {
        "idPedido": 1,
        "idCliente": 1,
        "nombreCliente": "María González",
        "idUsuario": 1,
        "nombreUsuario": "Laura Administradora",
        "idEstadoPedido": 3,
        "nombreEstadoPedido": "Completado",
        "idMetodoPago": 1,
        "nombreMetodoPago": "Transferencia",
        "fechaPedido": "2025-10-06T10:30:00",
        "subtotalPedido": 8500.00,
        "descuento": 0.00,
        "totalPedido": 8500.00,
        "detalles": [
            {
                "idDetalle": 1,
                "idPedido": 1,
                "idProducto": 1,
                "nombreProducto": "Bolso Clásico Negro",
                "cantidadProducto": 1,
                "precioUnitarioProducto": 8500.00,
                "subtotalProducto": 8500.00
            }
        ]
    },
    {
        "idPedido": 2,
        "idCliente": 2,
        "nombreCliente": "Ana Rodríguez",
        "idUsuario": 2,
        "nombreUsuario": "Carlos Vendedor",
        "idEstadoPedido": 1,
        "nombreEstadoPedido": "Pendiente",
        "idMetodoPago": 2,
        "nombreMetodoPago": "Efectivo",
        "fechaPedido": "2025-10-06T14:15:00",
        "subtotalPedido": 12300.00,
        "descuento": 5.00,
        "totalPedido": 11685.00,
        "detalles": [
            {
                "idDetalle": 2,
                "idPedido": 2,
                "idProducto": 2,
                "nombreProducto": "Cartera Mini Rosa",
                "cantidadProducto": 1,
                "precioUnitarioProducto": 4500.00,
                "subtotalProducto": 4500.00
            },
            {
                "idDetalle": 3,
                "idPedido": 2,
                "idProducto": 5,
                "nombreProducto": "Clutch Dorado",
                "cantidadProducto": 1,
                "precioUnitarioProducto": 7800.00,
                "subtotalProducto": 7800.00
            }
        ]
    }
]
```

### POST /api/pedidos

**Request:**
```json
{
    "idCliente": 1,
    "idUsuario": 1,
    "idEstadoPedido": 1,
    "idMetodoPago": 1,
    "subtotalPedido": 9200.00,
    "descuento": 5.00,
    "totalPedido": 8740.00,
    "detalles": [
        {
            "idProducto": 4,
            "cantidadProducto": 1,
            "precioUnitarioProducto": 9200.00,
            "subtotalProducto": 9200.00
        }
    ]
}
```

**Response 201 Created:**
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

**Response 400 Bad Request (Stock insuficiente):**
```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "Detalles": [
            "Stock insuficiente para el producto Mochila Urbana. Stock disponible: 5, solicitado: 10"
        ]
    },
    "traceId": "00-..."
}
```

**Response 400 Bad Request (Descuento inválido):**
```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "Descuento": [
            "El descuento debe estar entre 0 y 100."
        ]
    },
    "traceId": "00-..."
}
```

---

## Clientes

### GET /api/clientes

**Response 200 OK:**
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
    },
    {
        "idCliente": 2,
        "nombreCliente": "Ana Rodríguez",
        "telefonoCliente": "+54 9 11 3456-7890",
        "direccionCliente": "Avenida Central 456",
        "emailCliente": "ana.r@email.com",
        "categoriaCliente": "Frecuente",
        "totalGastado": 98400.00,
        "fechaUltimoPedido": "2025-10-04T14:15:00",
        "totalPedidos": 12
    },
    {
        "idCliente": 3,
        "nombreCliente": "Lucía Fernández",
        "telefonoCliente": "+54 9 11 4567-8901",
        "direccionCliente": "Boulevard Norte 789",
        "emailCliente": "lucia.f@email.com",
        "categoriaCliente": "Regular",
        "totalGastado": 64200.00,
        "fechaUltimoPedido": "2025-10-03T09:20:00",
        "totalPedidos": 8
    }
]
```

### POST /api/clientes

**Request:**
```json
{
    "nombreCliente": "Nuevo Cliente",
    "telefonoCliente": "+54 9 11 9999-9999",
    "direccionCliente": "Calle Nueva 123",
    "emailCliente": "nuevo@email.com"
}
```

**Response 400 Bad Request (Email duplicado):**
```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "EmailCliente": [
            "El email ya está en uso."
        ]
    },
    "traceId": "00-..."
}
```

---

## Productos

### GET /api/productos

**Response 200 OK:**
```json
[
    {
        "idProducto": 1,
        "nombreProducto": "Bolso Clásico Negro",
        "descripcionProducto": "Bolso elegante de cuero negro",
        "precioProducto": 8500.00,
        "stockProducto": 15,
        "estadoProducto": "Activo",
        "imagenProducto": "/images/bolso-negro.jpg",
        "idCategoria": 1,
        "nombreCategoria": "Bolsos"
    },
    {
        "idProducto": 4,
        "nombreProducto": "Mochila Urbana",
        "descripcionProducto": "Mochila moderna para uso diario",
        "precioProducto": 9200.00,
        "stockProducto": 12,
        "estadoProducto": "Activo",
        "imagenProducto": "/images/mochila-urbana.jpg",
        "idCategoria": 3,
        "nombreCategoria": "Mochilas"
    }
]
```

---

## Usuarios

### GET /api/usuarios

**Response 200 OK:**
```json
[
    {
        "idUsuario": 1,
        "nombreUsuario": "Laura Administradora",
        "emailUsuario": "admin@tritote.com.ni",
        "estadoUsuario": "Activo",
        "fechaCreacionUsuario": "2025-01-01T00:00:00",
        "ultimoAcceso": "2025-11-10T14:30:00",
        "idRol": 1,
        "nombreRol": "Administrador"
    },
    {
        "idUsuario": 2,
        "nombreUsuario": "Carlos Vendedor",
        "emailUsuario": "vendedor@tritote.com.ni",
        "estadoUsuario": "Activo",
        "fechaCreacionUsuario": "2025-01-01T00:00:00",
        "ultimoAcceso": "2025-11-10T14:15:00",
        "idRol": 2,
        "nombreRol": "Vendedor"
    }
]
```

---

## Errores Comunes

### 401 Unauthorized (Token inválido/expirado)
```json
{
    "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
    "title": "Unauthorized",
    "status": 401,
    "detail": "Token inválido o expirado"
}
```

### 404 Not Found
```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
    "title": "Not Found",
    "status": 404,
    "detail": "Pedido no encontrado."
}
```

### 500 Internal Server Error
```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
    "title": "Internal Server Error",
    "status": 500,
    "detail": "Error interno del servidor al obtener los datos."
}
```

---

## Notas Importantes

1. **Fechas**: Todas las fechas están en formato ISO 8601 (UTC).

2. **Decimales**: Los valores monetarios usan 2 decimales (ej: 8500.00).

3. **Nullables**: Los campos marcados como `?` en los DTOs pueden ser `null` en las respuestas JSON.

4. **Arrays vacíos**: Si no hay datos, se retorna un array vacío `[]` en lugar de `null`.

5. **Validaciones**: Los errores de validación incluyen el campo específico y el mensaje de error.

