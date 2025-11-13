using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Clases;
using SharedModels.Dto;
using TritoteNic.Data;

namespace TritoteNic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private readonly TritoteContext.TritoteConext _context;
        private readonly ILogger<CategoriaController> _logger;
        private readonly IMapper _mapper;

        public CategoriaController(TritoteContext.TritoteConext context, ILogger<CategoriaController> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetCategorias()
        {
            try
            {
                _logger.LogInformation("Obteniendo las Categorías");
                var categorias = await _context.Set<Categoria>().ToListAsync();
                return Ok(_mapper.Map<IEnumerable<CategoriaDto>>(categorias));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener las categorías: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al obtener las categorías.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CategoriaDto>> GetCategoria(int id)
        {
            if (id <= 0)
            {
                _logger.LogError($"ID de Categoría no válido: {id}");
                return BadRequest("ID de Categoría no válido.");
            }

            try
            {
                _logger.LogInformation($"Obteniendo Categoría con ID: {id}");
                var categoria = await _context.Set<Categoria>().FindAsync(id);

                if (categoria == null)
                {
                    _logger.LogWarning($"No se encontró ninguna categoría con ID: {id}");
                    return NotFound("Categoría no encontrada.");
                }

                return Ok(_mapper.Map<CategoriaDto>(categoria));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener categoría con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al obtener la categoría.");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CategoriaDto>> PostCategoria(CategoriaCreateDto createDto)
        {
            if (createDto == null)
            {
                _logger.LogError("Se recibió una categoría nula.");
                return BadRequest("La categoría no puede ser nula.");
            }

            try
            {
                _logger.LogInformation($"Creando nueva categoría: {createDto.NombreCategoria}");

                // Verificar duplicado
                var existing = await _context.Set<Categoria>()
                    .FirstOrDefaultAsync(c => c.NombreCategoria == createDto.NombreCategoria);
                if (existing != null)
                {
                    _logger.LogWarning($"La categoría '{createDto.NombreCategoria}' ya existe.");
                    ModelState.AddModelError("NombreCategoria", "El nombre de la categoría ya está en uso.");
                    return BadRequest(ModelState);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogError("Modelo de categoría no válido.");
                    return BadRequest(ModelState);
                }

                var categoria = _mapper.Map<Categoria>(createDto);
                _context.Set<Categoria>().Add(categoria);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Categoría creada con ID: {categoria.IdCategoria}");
                return CreatedAtAction(nameof(GetCategoria), new { id = categoria.IdCategoria }, _mapper.Map<CategoriaDto>(categoria));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear categoría: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al crear la categoría.");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutCategoria(int id, CategoriaUpdateDto updateDto)
        {
            if (updateDto == null || id != updateDto.IdCategoria)
            {
                return BadRequest("Datos de entrada no válidos o ID no coincide.");
            }

            try
            {
                _logger.LogInformation($"Actualizando categoría ID: {id}");

                var categoriaExistente = await _context.Set<Categoria>().FindAsync(id);
                if (categoriaExistente == null)
                {
                    _logger.LogWarning($"No se encontró categoría con ID: {id}");
                    return NotFound("La categoría no existe.");
                }

                // Verificar duplicado de nombre si cambia
                if (!string.IsNullOrWhiteSpace(updateDto.NombreCategoria))
                {
                    var conflict = await _context.Set<Categoria>()
                        .AnyAsync(c => c.NombreCategoria == updateDto.NombreCategoria && c.IdCategoria != id);
                    if (conflict)
                    {
                        ModelState.AddModelError("NombreCategoria", "El nombre ya está en uso por otra categoría.");
                        return BadRequest(ModelState);
                    }
                }

                _mapper.Map(updateDto, categoriaExistente);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Categoría ID {id} actualizada correctamente.");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!CategoriaExiste(id))
                {
                    _logger.LogWarning($"La categoría ID {id} no se encontró durante concurrencia.");
                    return NotFound("La categoría no se encontró durante la actualización.");
                }
                _logger.LogError($"Concurrencia al actualizar categoría ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno al actualizar la categoría.");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            try
            {
                _logger.LogInformation($"Eliminando categoría ID: {id}");
                var categoria = await _context.Set<Categoria>().FindAsync(id);
                if (categoria == null)
                {
                    _logger.LogWarning($"No se encontró categoría con ID: {id}");
                    return NotFound("Categoría no encontrada.");
                }

                _context.Set<Categoria>().Remove(categoria);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Categoría ID {id} eliminada correctamente.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar categoría ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno al eliminar la categoría.");
            }
        }

        private bool CategoriaExiste(int id) => _context.Set<Categoria>().Any(c => c.IdCategoria == id);
    }
}
