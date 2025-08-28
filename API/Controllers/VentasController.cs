using System.ComponentModel.DataAnnotations;
using API.Constants;
using API.Entities;
using API.DTOs;
using API.Extensions;
using API.Interfaces;
using API.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class VentasController(IVentaRepository ventaRepository) : BaseApiController
    {
        protected int KioscoId => User.GetKioscoId();
        protected int UserId => User.GetUserId();

        /// <summary>
        /// Obtiene las ventas del día actual o de una fecha específica.
        /// </summary>
        /// <param name="fecha">Fecha de la cual obtener las ventas (opcional, si no se especifica se usa el día actual).</param>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        /// <returns>Lista de ventas del día actual o de la fecha especificada.</returns>
        /// <response code="200">Retorna la lista de ventas.</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        [ProducesResponseType(typeof(IReadOnlyList<VentaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [HttpGet("dia")]
        public async Task<ActionResult<IReadOnlyList<VentaDto>>> GetVentasDelDia([FromQuery] DateTime? fecha, CancellationToken cancellationToken)
        {
            var ventas = await ventaRepository.GetVentasDelDiaAsync(KioscoId, cancellationToken, fecha);
            return ventas.ToActionResult();
        }

        /// <summary>
        /// Obtiene las ventas más recientes.
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        /// <param name="cantidad">Cantidad de ventas a obtener (por defecto 4, debe estar entre 1 y 10).</param>
        /// <returns>Lista de las ventas más recientes.</returns>
        /// <response code="200">Retorna la lista de ventas más recientes.</response>
        /// <response code="400">Cantidad inválida (debe estar entre 1 y 10).</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        [ProducesResponseType(typeof(IReadOnlyList<VentaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [HttpGet("recientes")]
        public async Task<ActionResult<IReadOnlyList<VentaDto>>> GetVentasRecientes(
            CancellationToken cancellationToken,
            [FromQuery, Range(1, 10, ErrorMessage = "La cantidad debe estar entre 1 y 10.")] int cantidad = 4)
        {
            var ventas = await ventaRepository.GetVentasRecientesAsync(KioscoId, cantidad, cancellationToken);
            return ventas.ToActionResult();
        }


        /// <summary>
        /// Obtiene el monto total de ventas del día actual o de una fecha específica.
        /// </summary>
        /// <param name="fecha">Fecha de la cual obtener el total de ventas (opcional, si no se especifica se usa el día actual).</param>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        /// <returns>Total en dinero de las ventas del día actual o de la fecha especificada.</returns>
        /// <response code="200">Retorna el monto total de ventas.</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [HttpGet("total-dia")]
        public async Task<ActionResult<decimal>> GetMontoTotalVentas([FromQuery] DateTime? fecha, CancellationToken cancellationToken)
        {
            var total = await ventaRepository.GetMontoTotalVentasDelDiaAsync(KioscoId, cancellationToken, fecha);
            return total.ToActionResult();
        }


        /// <summary>
        /// Obtiene los productos más vendidos del día actual o de una fecha específica.
        /// </summary>
        /// <param name="fecha">Fecha de la cual obtener los productos más vendidos (opcional, si no se especifica se usa el día actual).</param>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        /// <param name="cantidad">Cantidad de productos a obtener (por defecto 4, debe estar entre 1 y 10).</param>
        /// <returns>Lista de los productos más vendidos del día actual o de la fecha especificada.</returns>
        /// <response code="200">Retorna la lista de productos más vendidos.</response>
        /// <response code="400">Cantidad inválida (debe estar entre 1 y 10).</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        [ProducesResponseType(typeof(IReadOnlyList<ProductoMasVendidoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [HttpGet("productos-mas-vendidos")]
        public async Task<ActionResult<IReadOnlyList<ProductoMasVendidoDto>>> GetProductosMasVendidosDelDia(
            [FromQuery] DateTime? fecha,
            CancellationToken cancellationToken,
            [FromQuery, Range(1, 10, ErrorMessage = "La cantidad debe estar entre 1 y 10.")] int cantidad = 4)
        {
            var productos = await ventaRepository.GetProductosMasVendidosDelDiaAsync(KioscoId, cantidad, cancellationToken, fecha);
            return productos.ToActionResult();
        }

        /// <summary>
        /// Finaliza una venta registrando los productos vendidos y actualizando el stock.
        /// </summary>
        /// <param name="ventaDto">Información de la venta a finalizar.</param>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        /// <returns>La venta recién creada.</returns>
        /// <response code="201">Venta creada con éxito.</response>
        /// <response code="400">Datos de venta no válidos.</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="404">Uno o más productos no encontrados.</response>
        /// <response code="409">Stock insuficiente para uno o más productos.</response>
        [ProducesResponseType(typeof(VentaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status409Conflict)]
        [HttpPost("finalizar")]
        public async Task<ActionResult<VentaDto>> FinalizarVenta(VentaCreateDto ventaDto, CancellationToken cancellationToken)
        {
            var result = await ventaRepository.CreateVentaAsync(ventaDto, KioscoId, UserId, cancellationToken);

            return result.ToActionResult(venta => CreatedAtAction(nameof(GetVentasDelDia), new { fecha = venta.Fecha.Date }, venta));
        }

        /// <summary>
        /// Exporta ventas en un rango de fechas y/o con límite de registros.
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        /// <param name="fechaInicio">Fecha de inicio del rango (opcional).</param>
        /// <param name="fechaFin">Fecha de fin del rango (opcional).</param>
        /// <param name="limite">Límite de registros a exportar (opcional, máximo 5000).</param>
        /// <returns>Lista de ventas exportadas.</returns>
        /// <response code="200">Retorna la lista de ventas exportadas.</response>
        /// <response code="400">Parámetros no válidos (fechas, límites).</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="403">Prohibido. Se requiere rol de administrador.</response>
        [ProducesResponseType(typeof(IAsyncEnumerable<VentaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status403Forbidden)]
        [HttpGet("export")]
        [Authorize(Policy = "RequireAdminRole")]
        public ActionResult<IAsyncEnumerable<VentaDto>> GetVentasForExport(
            CancellationToken cancellationToken,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery, Range(1, 5000, ErrorMessage = "El límite debe estar entre 1 y 5000.")] int? limite = null)
        {
            if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio > fechaFin)
            {
                return Result<IAsyncEnumerable<VentaDto>>.Failure(ErrorCodes.ValidationError, "La fecha de inicio no puede ser posterior a la fecha de fin.").ToActionResult();
            }
            
            var result = ventaRepository.GetVentasForExportAsync(KioscoId, cancellationToken, fechaInicio, fechaFin, limite);
            return result.ToActionResult();
        }
    }
}
