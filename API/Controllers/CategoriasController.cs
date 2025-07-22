using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize(Policy = "RequireAdminRole")]
    public class CategoriasController : BaseApiController
    {
        private readonly ICategoriaRepository _categoriaRepository;

        public CategoriasController(ICategoriaRepository categoriaRepository)
        {
            _categoriaRepository = categoriaRepository;
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<CategoriaDto>>> GetCategorias(
            CancellationToken cancellationToken,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var categorias = await _categoriaRepository.GetCategoriasAsync(pageNumber, pageSize, cancellationToken);

            Response.AddPaginationHeader(categorias);

            return Ok(categorias);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaDto>> GetCategoria(int id, CancellationToken cancellationToken)
        {
            var categoria = await _categoriaRepository.GetCategoriaByIdAsync(id, cancellationToken);

            if (categoria == null)
            {
                return NotFound("Categoría no encontrada");
            }

            return Ok(categoria);
        }

        [HttpPost]
        public async Task<ActionResult<CategoriaDto>> CreateCategoria(CategoriaCreateDto createDto, CancellationToken cancellationToken)
        {
            var result = await _categoriaRepository.CreateCategoriaAsync(createDto, cancellationToken);

            return result.ToActionResult(categoria => CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, categoria));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategoria(int id, CategoriaUpdateDto updateDto, CancellationToken cancellationToken)
        {
            var result = await _categoriaRepository.UpdateCategoriaAsync(id, updateDto, cancellationToken);
            return result.ToActionResult();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoria(int id, CancellationToken cancellationToken)
        {
            var result = await _categoriaRepository.DeleteCategoriaAsync(id, cancellationToken);

            return result.ToActionResult();

        }
    }
}
