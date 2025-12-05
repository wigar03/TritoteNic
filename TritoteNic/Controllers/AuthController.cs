using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Clases;
using SharedModels.Dto;
using System.Security.Cryptography;
using System.Text;
using TritoteNic.Data;
using TritoteNic.Services;

namespace TritoteNic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] // El login no requiere autenticación
    public class AuthController : ControllerBase
    {
        private readonly TritoteContext.TritoteConext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthController(
            TritoteContext.TritoteConext context,
            ILogger<AuthController> logger,
            IJwtService jwtService,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
                {
                    return Unauthorized("Email y contraseña son requeridos.");
                }

                _logger.LogInformation($"Intento de login para email: {loginDto.Email}");

                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.EmailUsuario == loginDto.Email);

                if (usuario == null)
                {
                    _logger.LogWarning($"Usuario no encontrado: {loginDto.Email}");
                    return Unauthorized("Credenciales inválidas.");
                }

                if (usuario.EstadoUsuario != "Activo")
                {
                    _logger.LogWarning($"Intento de login con usuario inactivo: {loginDto.Email}");
                    return Unauthorized("Usuario inactivo.");
                }

                var hashedPassword = HashPassword(loginDto.Password);
                if (usuario.ContrasenaUsuario != hashedPassword && usuario.ContrasenaUsuario != loginDto.Password)
                {
                    _logger.LogWarning($"Contraseña incorrecta para: {loginDto.Email}");
                    return Unauthorized("Credenciales inválidas.");
                }

                usuario.UltimoAcceso = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var jwtKey = _configuration["Jwt:Key"] ?? "CHANGE_ME_IN_PRODUCTION";
                var jwtIssuer = _configuration["Jwt:Issuer"] ?? "TritoteNic";
                var jwtAudience = _configuration["Jwt:Audience"] ?? "TritoteNic_Users";
                var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

                var token = _jwtService.GenerateToken(usuario, jwtKey, jwtIssuer, jwtAudience, expirationMinutes);

                var response = new LoginResponseDto
                {
                    Token = token,
                    Usuario = new UsuarioDto
                    {
                        IdUsuario = usuario.IdUsuario,
                        NombreUsuario = usuario.NombreUsuario,
                        EmailUsuario = usuario.EmailUsuario,
                        EstadoUsuario = usuario.EstadoUsuario,
                        FechaCreacionUsuario = usuario.FechaCreacionUsuario,
                        UltimoAcceso = usuario.UltimoAcceso,
                        IdRol = usuario.IdRol,
                        NombreRol = usuario.Rol?.NombreRol
                    },
                    ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
                };

                _logger.LogInformation($"Login exitoso para: {loginDto.Email}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al procesar login: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno del servidor.");
            }
        }

        [HttpPost("initialize")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> InitializeDatabase()
        {
            try
            {
                // Verificar si ya hay datos
                var existingUsers = await _context.Usuarios.AnyAsync();
                if (existingUsers)
                {
                    return BadRequest(new { message = "La base de datos ya tiene datos. La inicialización solo se puede hacer cuando está vacía." });
                }

                _logger.LogInformation("Inicializando base de datos con datos completos de ejemplo");

                // 1. Crear Roles
                var rolAdmin = new Rol { NombreRol = "Administrador", DescripcionRol = "Rol con acceso completo al sistema" };
                var rolVendedor = new Rol { NombreRol = "Vendedor", DescripcionRol = "Rol con acceso limitado para ventas" };
                _context.Roles.AddRange(rolAdmin, rolVendedor);
                await _context.SaveChangesAsync();

                // 2. Crear Usuarios
                var usuarioAdmin = new Usuario
                {
                    NombreUsuario = "William Garcia",
                    EmailUsuario = "william.garcia@tritote.com.ni",
                    ContrasenaUsuario = "admin123",
                    IdRol = rolAdmin.IdRol,
                    EstadoUsuario = "Activo",
                    FechaCreacionUsuario = DateTime.UtcNow
                };
                var usuarioVendedor = new Usuario
                {
                    NombreUsuario = "Andres Gonzalez",
                    EmailUsuario = "andres.gonzalez@tritote.com.ni",
                    ContrasenaUsuario = "seller123",
                    IdRol = rolVendedor.IdRol,
                    EstadoUsuario = "Activo",
                    FechaCreacionUsuario = DateTime.UtcNow
                };
                _context.Usuarios.AddRange(usuarioAdmin, usuarioVendedor);
                await _context.SaveChangesAsync();

                // 3. Crear Categorías
                var categorias = new List<Categoria>
                {
                    new Categoria { NombreCategoria = "Canvas", DescripcionCategoria = "Bolsas de tela canvas" },
                    new Categoria { NombreCategoria = "Ecológico", DescripcionCategoria = "Materiales ecológicos y sostenibles" },
                    new Categoria { NombreCategoria = "Premium", DescripcionCategoria = "Productos de alta calidad" },
                    new Categoria { NombreCategoria = "Playa", DescripcionCategoria = "Diseños para playa y verano" },
                    new Categoria { NombreCategoria = "Market", DescripcionCategoria = "Bolsas para mercado" },
                    new Categoria { NombreCategoria = "Minimalista", DescripcionCategoria = "Diseños minimalistas y elegantes" }
                };
                _context.Categorias.AddRange(categorias);
                await _context.SaveChangesAsync();

                // 4. Crear Productos
                var productos = new List<Producto>
                {
                    new Producto { NombreProducto = "Tote Bag Clásico Beige", DescripcionProducto = "Bolsa clásica en color beige", PrecioProducto = 200, StockProducto = 25, IdCategoria = categorias[0].IdCategoria, EstadoProducto = "Activo", ImagenProducto = "https://images.unsplash.com/photo-1574365569389-a10d488ca3fb?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=400&h=400&fit=crop" },
                    new Producto { NombreProducto = "Tote Bag Eco Natural", DescripcionProducto = "Bolsa ecológica de materiales naturales", PrecioProducto = 250, StockProducto = 18, IdCategoria = categorias[1].IdCategoria, EstadoProducto = "Activo", ImagenProducto = "https://images.unsplash.com/photo-1758708536099-9f46dc81fffc?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=400&h=400&fit=crop" },
                    new Producto { NombreProducto = "Tote Bag Shopping Negro", DescripcionProducto = "Bolsa shopping en color negro", PrecioProducto = 220, StockProducto = 30, IdCategoria = categorias[0].IdCategoria, EstadoProducto = "Activo", ImagenProducto = "https://images.unsplash.com/photo-1759463408569-c81a969a6c3d?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=400&h=400&fit=crop" },
                    new Producto { NombreProducto = "Tote Bag Playero Azul", DescripcionProducto = "Bolsa playera en color azul", PrecioProducto = 300, StockProducto = 15, IdCategoria = categorias[3].IdCategoria, EstadoProducto = "Activo", ImagenProducto = "https://images.unsplash.com/photo-1465742744535-bac796b80f3a?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=400&h=400&fit=crop" },
                    new Producto { NombreProducto = "Tote Bag Minimalista Blanco", DescripcionProducto = "Bolsa minimalista en color blanco", PrecioProducto = 280, StockProducto = 22, IdCategoria = categorias[5].IdCategoria, EstadoProducto = "Activo", ImagenProducto = "https://images.unsplash.com/photo-1647426112650-6c96ce7ab5f0?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=400&h=400&fit=crop" },
                    new Producto { NombreProducto = "Tote Bag Market Kraft", DescripcionProducto = "Bolsa market en kraft", PrecioProducto = 180, StockProducto = 28, IdCategoria = categorias[4].IdCategoria, EstadoProducto = "Activo", ImagenProducto = "https://images.unsplash.com/photo-1663154438413-244fae34c9a7?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=400&h=400&fit=crop" },
                    new Producto { NombreProducto = "Tote Bag Premium Gris", DescripcionProducto = "Bolsa premium en color gris", PrecioProducto = 450, StockProducto = 12, IdCategoria = categorias[2].IdCategoria, EstadoProducto = "Activo", ImagenProducto = "https://images.unsplash.com/photo-1574365569389-a10d488ca3fb?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=400&h=400&fit=crop" },
                    new Producto { NombreProducto = "Tote Bag Organic Verde", DescripcionProducto = "Bolsa orgánica en color verde", PrecioProducto = 320, StockProducto = 20, IdCategoria = categorias[1].IdCategoria, EstadoProducto = "Activo", ImagenProducto = "https://images.unsplash.com/photo-1758708536099-9f46dc81fffc?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=400&h=400&fit=crop" }
                };
                _context.Productos.AddRange(productos);
                await _context.SaveChangesAsync();

                // 5. Crear Estados de Pedido
                var estadosPedido = new List<EstadoPedido>
                {
                    new EstadoPedido { NombreEstadoPedido = "Pendiente", DescripcionEstadoPedido = "Pedido pendiente de procesamiento" },
                    new EstadoPedido { NombreEstadoPedido = "En Proceso", DescripcionEstadoPedido = "Pedido en proceso de preparación" },
                    new EstadoPedido { NombreEstadoPedido = "Completado", DescripcionEstadoPedido = "Pedido completado y entregado" },
                    new EstadoPedido { NombreEstadoPedido = "Cancelado", DescripcionEstadoPedido = "Pedido cancelado" },
                    new EstadoPedido { NombreEstadoPedido = "Retrasado", DescripcionEstadoPedido = "Pedido retrasado" }
                };
                _context.EstadosPedidos.AddRange(estadosPedido);
                await _context.SaveChangesAsync();

                // 6. Crear Métodos de Pago
                var metodosPago = new List<MetodoPago>
                {
                    new MetodoPago { NombreMetodoPago = "Efectivo", DescripcionMetodoPago = "Pago en efectivo" },
                    new MetodoPago { NombreMetodoPago = "Tarjeta", DescripcionMetodoPago = "Pago con tarjeta de crédito/débito" },
                    new MetodoPago { NombreMetodoPago = "Transferencia", DescripcionMetodoPago = "Transferencia bancaria" }
                };
                _context.MetodosPago.AddRange(metodosPago);
                await _context.SaveChangesAsync();

                // 7. Crear Clientes
                var clientes = new List<Cliente>
                {
                    new Cliente { NombreCliente = "Maria Castillo", EmailCliente = "maria.castillo@gmail.com", TelefonoCliente = "+505 8765-4321", DireccionCliente = "Managua, Nicaragua", TotalGastado = 25 },
                    new Cliente { NombreCliente = "Carlos Mendoza", EmailCliente = "carlos.mendoza@gmail.com", TelefonoCliente = "+505 7654-3210", DireccionCliente = "Managua, Nicaragua", TotalGastado = 18 },
                    new Cliente { NombreCliente = "Ana Sanchez", EmailCliente = "ana.sanchez@gmail.com", TelefonoCliente = "+505 8123-4567", DireccionCliente = "Managua, Nicaragua", TotalGastado = 30 },
                    new Cliente { NombreCliente = "Roberto Lopez", EmailCliente = "roberto.lopez@gmail.com", TelefonoCliente = "+505 8234-5678", DireccionCliente = "Managua, Nicaragua", TotalGastado = 12 },
                    new Cliente { NombreCliente = "Sofia Ramirez", EmailCliente = "sofia.ramirez@gmail.com", TelefonoCliente = "+505 8345-6789", DireccionCliente = "Managua, Nicaragua", TotalGastado = 20 }
                };
                _context.Clientes.AddRange(clientes);
                await _context.SaveChangesAsync();

                // 8. Crear Pedidos con detalles (distribuidos en diferentes fechas para estadísticas)
                var ahora = DateTime.UtcNow;
                var pedidos = new List<Pedido>
                {
                    // Pedidos recientes (esta semana)
                    new Pedido { IdCliente = clientes[0].IdCliente, IdUsuario = usuarioAdmin.IdUsuario, IdEstadoPedido = estadosPedido[0].IdEstadoPedido, IdMetodoPago = metodosPago[2].IdMetodoPago, FechaPedido = ahora.AddDays(-1), TotalPedido = 8500 },
                    new Pedido { IdCliente = clientes[1].IdCliente, IdUsuario = usuarioVendedor.IdUsuario, IdEstadoPedido = estadosPedido[1].IdEstadoPedido, IdMetodoPago = metodosPago[0].IdMetodoPago, FechaPedido = ahora.AddDays(-1), TotalPedido = 12300 },
                    new Pedido { IdCliente = clientes[2].IdCliente, IdUsuario = usuarioAdmin.IdUsuario, IdEstadoPedido = estadosPedido[2].IdEstadoPedido, IdMetodoPago = metodosPago[1].IdMetodoPago, FechaPedido = ahora.AddDays(-2), TotalPedido = 6700 },
                    new Pedido { IdCliente = clientes[3].IdCliente, IdUsuario = usuarioVendedor.IdUsuario, IdEstadoPedido = estadosPedido[4].IdEstadoPedido, IdMetodoPago = metodosPago[2].IdMetodoPago, FechaPedido = ahora.AddDays(-2), TotalPedido = 9200 },
                    new Pedido { IdCliente = clientes[4].IdCliente, IdUsuario = usuarioAdmin.IdUsuario, IdEstadoPedido = estadosPedido[2].IdEstadoPedido, IdMetodoPago = metodosPago[0].IdMetodoPago, FechaPedido = ahora.AddDays(-3), TotalPedido = 15400 },
                    new Pedido { IdCliente = clientes[0].IdCliente, IdUsuario = usuarioVendedor.IdUsuario, IdEstadoPedido = estadosPedido[1].IdEstadoPedido, IdMetodoPago = metodosPago[1].IdMetodoPago, FechaPedido = ahora.AddDays(-3), TotalPedido = 7800 },
                    new Pedido { IdCliente = clientes[1].IdCliente, IdUsuario = usuarioAdmin.IdUsuario, IdEstadoPedido = estadosPedido[0].IdEstadoPedido, IdMetodoPago = metodosPago[2].IdMetodoPago, FechaPedido = ahora.AddDays(-4), TotalPedido = 10500 },
                    new Pedido { IdCliente = clientes[2].IdCliente, IdUsuario = usuarioVendedor.IdUsuario, IdEstadoPedido = estadosPedido[2].IdEstadoPedido, IdMetodoPago = metodosPago[0].IdMetodoPago, FechaPedido = ahora.AddDays(-4), TotalPedido = 13200 }
                };
                _context.Pedidos.AddRange(pedidos);
                await _context.SaveChangesAsync();

                // 9. Crear Detalles de Pedidos (asociar productos a pedidos)
                var detallesPedido = new List<DetallePedido>();
                var random = new Random();
                
                foreach (var pedido in pedidos)
                {
                    // Cada pedido tiene 1-3 productos
                    var numProductos = random.Next(1, 4);
                    var productosPedido = productos.OrderBy(x => random.Next()).Take(numProductos).ToList();
                    
                    foreach (var producto in productosPedido)
                    {
                        var cantidad = random.Next(1, 4);
                        var precioUnitario = producto.PrecioProducto;
                        var subtotal = precioUnitario * cantidad;
                        
                        detallesPedido.Add(new DetallePedido
                        {
                            IdPedido = pedido.IdPedido,
                            IdProducto = producto.IdProducto,
                            CantidadProducto = cantidad,
                            PrecioUnitarioProducto = precioUnitario,
                            SubtotalProducto = subtotal
                        });
                    }
                }
                _context.DetallesPedido.AddRange(detallesPedido);
                await _context.SaveChangesAsync();

                // Actualizar fechas de último pedido de clientes
                foreach (var cliente in clientes)
                {
                    var ultimoPedido = pedidos.Where(p => p.IdCliente == cliente.IdCliente)
                        .OrderByDescending(p => p.FechaPedido)
                        .FirstOrDefault();
                    if (ultimoPedido != null)
                    {
                        cliente.FechaUltimoPedido = ultimoPedido.FechaPedido;
                    }
                }
                await _context.SaveChangesAsync();

                _logger.LogInformation("Base de datos inicializada completamente");

                return Ok(new
                {
                    message = "Base de datos inicializada correctamente con todos los datos de ejemplo",
                    rolesCreated = 2,
                    usersCreated = 2,
                    categoriasCreated = categorias.Count,
                    productosCreated = productos.Count,
                    estadosPedidoCreated = estadosPedido.Count,
                    metodosPagoCreated = metodosPago.Count,
                    clientesCreated = clientes.Count,
                    pedidosCreated = pedidos.Count,
                    detallesPedidoCreated = detallesPedido.Count,
                    users = new[]
                    {
                        new { email = usuarioAdmin.EmailUsuario, role = "Administrador", password = "admin123" },
                        new { email = usuarioVendedor.EmailUsuario, role = "Vendedor", password = "seller123" }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al inicializar la base de datos: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al inicializar la base de datos", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}


