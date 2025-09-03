using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
    [Authorize]
    public class ReportesController(IReporteRepository reporteRepository, IVentaRepository ventaRepository) : BaseApiController
    {
        protected int KioscoId => User.GetKioscoId();

        /// <summary>
        /// Obtiene el reporte general de KPIs del kiosco
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación</param>
        /// <param name="dateRange">Rango de fechas para el reporte</param>
        /// <returns>Reporte con los KPIs principales del kiosco</returns>
        /// <response code="200">Reporte generado exitosamente</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="400">Parámetros de fecha inválidos</response>
        [HttpGet]
        [ProducesResponseType(typeof(ReporteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ReporteDto>> GetReporte(
            [FromQuery] DateRangeDto dateRange,
            CancellationToken cancellationToken)
        {
            var result = await reporteRepository.CalculateKpiReporteAsync(KioscoId, dateRange.FechaInicio, dateRange.FechaFin, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Obtiene los productos más vendidos en un período específico
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación</param>
        /// <param name="dateRange">Rango de fechas para el reporte</param>
        /// <param name="pageNumber">Número de página (por defecto: 1)</param>
        /// <param name="pageSize">Tamaño de página (por defecto: 10, máximo: 10)</param>
        /// <param name="limit">Límite de productos a retornar (por defecto: 5, máximo: 50)</param>
        /// <returns>Lista paginada de los productos más vendidos</returns>
        /// <response code="200">Lista de productos más vendidos obtenida exitosamente</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="400">Parámetros inválidos</response>
        [HttpGet("top-productos")]
        [ProducesResponseType(typeof(PagedList<ProductoMasVendidoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedList<ProductoMasVendidoDto>>> GetTopProductos(
            CancellationToken cancellationToken,
            [FromQuery] DateRangeDto dateRange,
            [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
            [FromQuery, Range(1, 10)] int pageSize = 10,
            [FromQuery, Range(1, 50)] int limit = 5)
        {
            var result = await reporteRepository.GetTopProductsByVentasAsync(
                KioscoId,
                pageNumber,
                pageSize,
                dateRange.FechaInicio,
                dateRange.FechaFin,
                cancellationToken,
                limit);

            return result.ToActionResult(topProducts =>
            {
                Response.AddPaginationHeader(topProducts);
                return Ok(topProducts);
            });
        }

        /// <summary>
        /// Obtiene las ventas agrupadas por día en un período específico
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación</param>
        /// <param name="dateRange">Rango de fechas para el reporte</param>
        /// <returns>Lista de ventas agrupadas por día</returns>
        /// <response code="200">Datos de ventas por día obtenidos exitosamente</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="400">Parámetros de fecha inválidos</response>
        [HttpGet("ventas-por-dia")]
        [ProducesResponseType(typeof(IReadOnlyList<VentasPorDiaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IReadOnlyList<VentasPorDiaDto>>> GetVentasPorDia(
            [FromQuery] DateRangeDto dateRange,
            CancellationToken cancellationToken)
        {
            var result = await reporteRepository.GetVentasPorDiaAsync(
                KioscoId,
                dateRange.FechaInicio,
                dateRange.FechaFin,
                cancellationToken);

            return result.ToActionResult();
        }

        /// <summary>
        /// Obtiene el análisis de rentabilidad por categorías
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación</param>
        /// <param name="dateRange">Rango de fechas para el reporte</param>
        /// <returns>Lista de categorías con su análisis de rentabilidad</returns>
        /// <response code="200">Análisis de rentabilidad por categorías obtenido exitosamente</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="400">Parámetros de fecha inválidos</response>
        [HttpGet("rentabilidad-categorias")]
        [ProducesResponseType(typeof(IReadOnlyList<CategoriasRentabilidadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IReadOnlyList<CategoriasRentabilidadDto>>> GetCategoriasRentabilidad(
            [FromQuery] DateRangeDto dateRange,
            CancellationToken cancellationToken)
        {
            var result = await reporteRepository.GetCategoriasRentabilidadAsync(KioscoId, dateRange.FechaInicio, dateRange.FechaFin, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Obtiene las ventas individuales del día para gráficos
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación</param>
        /// <param name="fecha">Fecha específica para consultar (opcional, por defecto: hoy)</param>
        /// <returns>Lista de ventas individuales del día especificado</returns>
        /// <response code="200">Datos de ventas para gráficos obtenidos exitosamente</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="400">Fecha inválida</response>
        [HttpGet("ventas-chart")]
        [ProducesResponseType(typeof(IReadOnlyList<VentaChartDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IReadOnlyList<VentaChartDto>>> GetVentasParaChart(
            [FromQuery] DateTime fecha,
            CancellationToken cancellationToken)
        {
            var result = await ventaRepository.GetVentasIndividualesDelDiaAsync(KioscoId, cancellationToken, fecha);
            return result.ToActionResult();
        }
    }
}
