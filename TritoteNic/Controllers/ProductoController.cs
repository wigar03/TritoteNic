using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Clases;
using SharedModels.Dto;
using TritoteNic.Data;
using TritoteNic.Services;

namespace TritoteNic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOrVendedor")]
    public class ProductoController : ControllerBase
    {
        private readonly TritoteContext.TritoteConext _context;
        private readonly ILogger<ProductoController> _logger;
        private readonly IMapper _mapper;
        private readonly Services.IBitacoraService _bitacoraService;

        public ProductoController(TritoteContext.TritoteConext context, ILogger<ProductoController> logger, IMapper mapper, Services.IBitacoraService bitacoraService)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
            _bitacoraService = bitacoraService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos()
        {
            try
            {
                _logger.LogInformation("Obteniendo los Productos");
                var productos = await _context.Productos
                    .Include(p => p.Categoria)
                    .ToListAsync();
                
                var productosDto = productos.Select(p => new ProductoDto
                {
                    IdProducto = p.IdProducto,
                    NombreProducto = p.NombreProducto,
                    DescripcionProducto = p.DescripcionProducto,
                    PrecioProducto = p.PrecioProducto,
                    StockProducto = p.StockProducto,
                    IdCategoria = p.IdCategoria,
                    NombreCategoria = p.Categoria?.NombreCategoria,
                    EstadoProducto = p.EstadoProducto,
                    ImagenProducto = p.ImagenProducto
                }).ToList();
                
                return Ok(productosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener los Productos: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al obtener los Productos.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductoDto>> GetProducto(int id)
        {
            if (id <= 0)
            {
                _logger.LogError($"ID de Producto no válido: {id}");
                return BadRequest("ID de Producto no válido.");
            }

            try
            {
                _logger.LogInformation($"Obteniendo Producto con ID: {id}");

                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                {
                    _logger.LogWarning($"No se encontró ningún producto con ID: {id}");
                    return NotFound("Producto no encontrado.");
                }

                return Ok(_mapper.Map<ProductoDto>(producto));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener producto con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al obtener el Producto.");
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductoDto>> PostProducto(ProductoCreateDto createDto)
        {
            if (createDto == null)
            {
                _logger.LogError("Se recibió un producto nulo en la solicitud.");
                return BadRequest("El producto no puede ser nulo.");
            }

            try
            {
                _logger.LogInformation($"Creando un nuevo producto con nombre: {createDto.NombreProducto}");

                // Verificar si el producto ya existe
                var existing = await _context.Productos
                    .FirstOrDefaultAsync(p => p.NombreProducto == createDto.NombreProducto);
                if (existing != null)
                {
                    _logger.LogWarning($"El producto con nombre {createDto.NombreProducto} ya existe.");
                    ModelState.AddModelError("NombreProducto", "El nombre del producto ya está en uso.");
                    return BadRequest(ModelState);
                }

                // Verificar la validez del modelo
                if (!ModelState.IsValid)
                {
                    _logger.LogError("El modelo de producto recibido no es válido.");
                    return BadRequest(ModelState);
                }

                // Crear el nuevo producto
                var producto = _mapper.Map<Producto>(createDto);
                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                // Registrar en bitácora
                await _bitacoraService.RegistrarCambioAsync(
                    tablaAfectada: "Producto",
                    accion: "CREATE",
                    idRegistro: producto.IdProducto,
                    descripcionRegistro: producto.NombreProducto,
                    datosAnteriores: null,
                    datosNuevos: createDto,
                    observaciones: $"Producto creado: {producto.NombreProducto} - Stock: {producto.StockProducto}");

                _logger.LogInformation($"Nuevo producto '{createDto.NombreProducto}' creado con ID: {producto.IdProducto}");
                return CreatedAtAction(nameof(GetProducto), new { id = producto.IdProducto }, _mapper.Map<ProductoDto>(producto));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear un nuevo producto: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al crear un nuevo producto.");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutProducto(int id, ProductoUpdateDto updateDto)
        {
            if (updateDto == null || id != updateDto.IdProducto)
            {
                return BadRequest("Los datos de entrada no son válidos o el ID del producto no coincide");
            }

            try
            {
                _logger.LogInformation($"Actualizando producto con ID: {id}");

                var productoExistente = await _context.Productos.FindAsync(id);
                if (productoExistente == null)
                {
                    _logger.LogWarning($"No se encontró ningún producto con ID: {id}");
                    return NotFound("El producto no existe.");
                }

                // Guardar datos anteriores para bitácora
                var datosAnteriores = new
                {
                    productoExistente.NombreProducto,
                    productoExistente.PrecioProducto,
                    productoExistente.StockProducto,
                    productoExistente.EstadoProducto
                };

                //Actualizar solo las propiedades necesarias del producto existente
                _mapper.Map(updateDto, productoExistente);
                await _context.SaveChangesAsync();

                // Registrar en bitácora
                await _bitacoraService.RegistrarCambioAsync(
                    tablaAfectada: "Producto",
                    accion: "UPDATE",
                    idRegistro: id,
                    descripcionRegistro: productoExistente.NombreProducto,
                    datosAnteriores: datosAnteriores,
                    datosNuevos: updateDto,
                    observaciones: $"Producto actualizado: {productoExistente.NombreProducto}");

                _logger.LogInformation($"Producto con ID {id} actualizado correctamente.");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ProductoExiste(id))
                {
                    _logger.LogWarning($"No se encontró ningún producto con ID: {id}");
                    return NotFound("El producto no se encontró durante la actualización.");
                }
                else
                {
                    _logger.LogError($"Error de concurrencia al actualizar el producto con ID: {id}. Detalles: {ex.Message}");
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        "Error interno del servidor al actualizar el producto.");
                }
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            try
            {
                _logger.LogInformation($"Eliminando producto con ID: {id}");

                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                {
                    _logger.LogWarning($"No se encontró ningún producto con ID: {id}");
                    return NotFound("Producto no encontrado.");
                }

                // Guardar datos antes de eliminar para bitácora
                var datosEliminados = new
                {
                    producto.NombreProducto,
                    producto.PrecioProducto,
                    producto.StockProducto
                };

                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();

                // Registrar en bitácora
                await _bitacoraService.RegistrarCambioAsync(
                    tablaAfectada: "Producto",
                    accion: "DELETE",
                    idRegistro: id,
                    descripcionRegistro: producto.NombreProducto,
                    datosAnteriores: datosEliminados,
                    datosNuevos: null,
                    observaciones: $"Producto eliminado: {producto.NombreProducto}");

                _logger.LogInformation($"Producto con ID {id} eliminado correctamente");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar el producto con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Se produjo un error al eliminar el producto.");
            }
        }

        [HttpPatch("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PatchProducto(int id, JsonPatchDocument<ProductoUpdateDto> patchDto)
        {
            if (id <= 0)
            {
                _logger.LogError($"ID de Producto no válido: {id}");
                return BadRequest("ID de Producto no válido.");
            }

            try
            {
                _logger.LogInformation($"Aplicando el parche al producto con ID: {id}");

                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                {
                    _logger.LogWarning($"No se encontró ningún producto con ID: {id}");
                    return NotFound("El producto no se encontró.");
                }

                // Guardar datos anteriores para bitácora
                var datosAnteriores = new
                {
                    producto.NombreProducto,
                    producto.PrecioProducto,
                    producto.StockProducto,
                    producto.EstadoProducto
                };

                var productoDto = _mapper.Map<ProductoUpdateDto>(producto);
                patchDto.ApplyTo(productoDto, ModelState);

                if (!ModelState.IsValid)
                {
                    _logger.LogError("El modelo de producto después de aplicar el parche no es válido.");
                    return BadRequest(ModelState);
                }

                _mapper.Map(productoDto, producto);

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        await _context.SaveChangesAsync();
                        transaction.Commit();
                        
                        // Registrar en bitácora
                        await _bitacoraService.RegistrarCambioAsync(
                            tablaAfectada: "Producto",
                            accion: "PATCH",
                            idRegistro: id,
                            descripcionRegistro: producto.NombreProducto,
                            datosAnteriores: datosAnteriores,
                            datosNuevos: productoDto,
                            observaciones: $"Producto actualizado parcialmente: {producto.NombreProducto}");
                        
                        _logger.LogInformation($"Parche aplicado correctamente al producto con ID: {id}");
                        return NoContent();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        transaction.Rollback();
                        if (!ProductoExiste(id))
                        {
                            _logger.LogWarning($"No se encontró ningún producto con ID: {id}");
                            return NotFound("El producto no se encontró durante la actualización.");
                        }
                        else
                        {
                            _logger.LogError($"Error de concurrencia al aplicar el parche al producto con ID: {id}. Detalles: {ex.Message}");
                            return StatusCode(StatusCodes.Status500InternalServerError,
                                "Error interno del servidor al aplicar el parche al producto.");
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"Error al aplicar el parche al producto con ID {id}: {ex.Message}");
                        return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error interno del servidor al aplicar el parche al producto.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al aplicar el parche al producto con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al aplicar el parche al producto.");
            }
        }

        private bool ProductoExiste(int id) => _context.Productos.Any(p => p.IdProducto == id);
    }
}
