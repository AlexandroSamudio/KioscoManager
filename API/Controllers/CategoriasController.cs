using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class CategoriasController : BaseApiController
    {
        private readonly ICategoriaRepository _categoriaRepository;

        public CategoriasController(ICategoriaRepository categoriaRepository)
        {
            _categoriaRepository = categoriaRepository;
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<CategoriaDto>>> GetCategorias(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var categorias = await _categoriaRepository.GetCategoriasAsync(pageNumber, pageSize, cancellationToken);
            
            Response.AddPaginationHeader(categorias);
            
            return Ok(categorias);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaDto>> GetCategoria(int id, CancellationToken cancellationToken = default)
        {
            var categoria = await _categoriaRepository.GetCategoriaByIdAsync(id, cancellationToken);
            
            if (categoria == null)
            {
                return NotFound("Categoría no encontrada");
            }

            return Ok(categoria);
        }

        [HttpPost]
        public async Task<ActionResult<CategoriaDto>> CreateCategoria(CategoriaCreateDto createDto, CancellationToken cancellationToken = default)
        {
            var categoria = await _categoriaRepository.CreateCategoriaAsync(createDto, cancellationToken);
            return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, categoria);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CategoriaDto>> UpdateCategoria(int id, CategoriaUpdateDto updateDto, CancellationToken cancellationToken = default)
        {
            await _categoriaRepository.UpdateCategoriaAsync(id, updateDto, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCategoria(int id, CancellationToken cancellationToken = default)
        {
            var result = await _categoriaRepository.DeleteCategoriaAsync(id, cancellationToken);

            if (!result)
            {
                return NotFound("Categoría no encontrada");
            }

            return NoContent();
        }
    }
}
