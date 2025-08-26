using API.DTOs;
using API.Extensions;
using API.Interfaces;
using API.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class ComprasController(ICompraService compraService) : BaseApiController
    {
        protected int KioscoId => User.GetKioscoId();

        /// <summary>
        /// Obtiene una compra por su id.
        /// </summary>
        /// <param name="id">El id de la compra.</param>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        /// <returns>La compra solicitada si se encuentra.</returns>
        /// <response code="200">Retorna la compra solicitada.</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="404">Compra no encontrada.</response>
        [ProducesResponseType(typeof(CompraDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<CompraDto>> GetCompra([FromRoute] int id, CancellationToken cancellationToken)
        {
            var result = await compraService.GetCompraByIdAsync(KioscoId, id, cancellationToken);

            return result.ToActionResult();
        }

        /// <summary>
        /// Crea una nueva compra.
        /// </summary>
        /// <param name="compraDto">Información de la compra.</param>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        /// <returns>La compra recién creada.</returns>
        /// <response code="201">Compra creada con éxito.</response>
        /// <response code="400">Datos de compra no válidos.</response>
        /// <response code="401">
        /// No autorizado. Se requiere un JWT válido.
        /// </response>
        /// <response code="404">Id de producto no encontrado.</response>
        [ProducesResponseType(typeof(CompraDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
        [HttpPost]
        public async Task<ActionResult<CompraDto>> CreateCompra(CompraCreateDto compraDto, CancellationToken cancellationToken)
        {
            var usuarioId = User.GetUserId();

            var result = await compraService.CreateCompraAsync(
                compraDto, KioscoId, usuarioId, cancellationToken);

            return result.ToActionResult(compra => CreatedAtAction(nameof(GetCompra), new { id = compra.Id }, compra));
        }

        /// <summary>
        /// Exporta compras en un rango de fechas y/o con límite de registros.
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        /// <param name="fechaInicio">Fecha de inicio (opcional).</param>
        /// <param name="fechaFin">Fecha de fin (opcional).</param>
        /// <param name="limite">Límite de registros (opcional).</param>
        /// <returns>Lista de compras exportadas.</returns>
        /// <response code="200">Retorna la lista de compras exportadas.</response>
        /// <response code="400">Parámetros no válidos.</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="403">Prohibido. Se requiere rol de administrador.</response>
        [ProducesResponseType(typeof(IReadOnlyList<CompraDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status403Forbidden)]
        [HttpGet("export")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult<IReadOnlyList<CompraDto>>> GetComprasForExport(
            CancellationToken cancellationToken,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int? limite = null)
        {
            var compras = await compraService.GetComprasForExportAsync(KioscoId, cancellationToken, fechaInicio, fechaFin, limite);
            return compras.ToActionResult();
        }
    }
}