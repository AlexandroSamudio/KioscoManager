using System.ComponentModel.DataAnnotations;
using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class CategoriasController(ICategoriaRepository categoriaRepository) : BaseApiController
    {

        /// <summary>
        /// Obtiene una lista paginada de categorías.
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        /// <param name="pageNumber">Número de página a recuperar (el valor predeterminado es 1).</param>
        /// <param name="pageSize">Número de elementos por página (el valor predeterminado es 10).</param>
        /// <returns>Una lista paginada que contiene elementos de categoría.</returns>
        /// <response code="200">Retorna la lista paginada de categorías.</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        [ProducesResponseType(typeof(PagedList<CategoriaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [HttpGet]
        public async Task<ActionResult<PagedList<CategoriaDto>>> GetCategorias(
            CancellationToken cancellationToken,
            [FromQuery, Range(1, 100)] int pageNumber = 1,
            [FromQuery, Range(1, 10)] int pageSize = 10)
        {
            var result = await categoriaRepository.GetCategoriasAsync(pageNumber, pageSize, cancellationToken);

            return result.ToActionResult(paged =>
            {
                Response.AddPaginationHeader(paged);
                return Ok(paged);
            });
        }

        /// <summary>
        /// Obtiene una categoría por su id.
        /// </summary>
        /// <param name="id">El id de la categoría.</param>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        /// <returns>La categoría solicitada si se encuentra.</returns>
        /// <response code="200">Retorna la categoría solicitada.</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="404">Categoría no encontrada.</response>
        [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaDto>> GetCategoria(int id, CancellationToken cancellationToken)
        {
            var result = await categoriaRepository.GetCategoriaByIdAsync(id, cancellationToken);

            return result.ToActionResult();
        }

        /// <summary>
        /// Crea una nueva categoría.
        /// </summary>
        /// <param name="createDto">Información de la categoría.</param>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        /// <returns>La categoría recién creada.</returns>
        /// <response code="201">Categoría creada con éxito.</response>
        /// <response code="400">Datos de categoría no válidos.</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="403">Prohibido. Se requiere rol de administrador.</response>
        /// <response code="409">Ya existe una categoría con los mismos datos únicos.</response>
        [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status409Conflict)]
        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost]
        public async Task<ActionResult<CategoriaDto>> CreateCategoria(CategoriaCreateDto createDto, CancellationToken cancellationToken)
        {
            var result = await categoriaRepository.CreateCategoriaAsync(createDto, cancellationToken);

            return result.ToActionResult(categoria => CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, categoria));
        }

        /// <summary>
        /// Actualiza una categoría existente.
        /// </summary>
        /// <param name="id">El id de la categoría.</param>
        /// <param name="updateDto">Información de la categoría actualizada.</param>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        /// <returns>Sin contenido en caso de éxito.</returns>
        /// <response code="204">Actualización completada con éxito.</response>
        /// <response code="400">Datos de categoría no válidos.</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="403">Prohibido. Se requiere rol de administrador.</response>
        /// <response code="404">Categoría no encontrada.</response>
        /// <response code="409">Ya existe una categoría con los mismos datos únicos.</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status409Conflict)]
        [Authorize(Policy = "RequireAdminRole")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategoria(int id, CategoriaUpdateDto updateDto, CancellationToken cancellationToken)
        {
            var result = await categoriaRepository.UpdateCategoriaAsync(id, updateDto, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Elimina una categoría por su id.
        /// </summary>
        /// <param name="id">El id de la categoría.</param>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        /// <returns>Sin contenido en caso de éxito.</returns>
        /// <response code="204">Eliminación completada con éxito.</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="403">Prohibido. Se requiere rol de administrador.</response>
        /// <response code="404">Categoría no encontrada.</response>
        /// <response code="409">Conflicto. No se puede eliminar una categoria con productos asociados.</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status409Conflict)]
        [Authorize(Policy = "RequireAdminRole")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoria(int id, CancellationToken cancellationToken)
        {
            var result = await categoriaRepository.DeleteCategoriaAsync(id, cancellationToken);

            return result.ToActionResult();
        }
    }
}
