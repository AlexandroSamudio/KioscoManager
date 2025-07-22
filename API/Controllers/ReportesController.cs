using API.DTOs;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class ReportesController(IReporteRepository reportRepository, IVentaRepository ventaRepository) : BaseApiController
    {
        protected int KioscoId => User.GetKioscoId();

        [HttpGet]
        public async Task<ActionResult<ReporteDto>> GetReporte(
            CancellationToken cancellationToken,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            var (fechaInicioFinal, fechaFinFinal) = NormalizarFechas(fechaInicio, fechaFin, 30);

            var summary = await reportRepository.CalculateKpiReporteAsync(KioscoId, fechaInicioFinal, fechaFinFinal, cancellationToken);
            return Ok(summary);
        }

        [HttpGet("top-productos")]
        public async Task<ActionResult<IReadOnlyList<ProductoMasVendidoDto>>> GetTopProductos(
            CancellationToken cancellationToken,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int limit = 5)
        {
            var (fechaInicioFinal, fechaFinFinal) = NormalizarFechas(fechaInicio, fechaFin, 90);

            limit = Math.Clamp(limit, 1, 50);
            pageSize = Math.Clamp(pageSize, 1, 10);
            pageNumber = Math.Max(pageNumber, 1);

            var topProducts = await reportRepository.GetTopProductsByVentasAsync(
                KioscoId,
                pageNumber,
                pageSize,
                fechaInicioFinal,
                fechaFinFinal,
                cancellationToken,
                limit);

            if (topProducts.Count == 0)
            {
                return Ok(new List<ProductoMasVendidoDto>());
            }

            Response.AddPaginationHeader(topProducts);
            return Ok(topProducts);
        }

        [HttpGet("ventas-por-dia")]
        public async Task<ActionResult<IReadOnlyList<VentasPorDiaDto>>> GetVentasPorDia(
            CancellationToken cancellationToken,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            var (fechaInicioFinal, fechaFinFinal) = NormalizarFechas(fechaInicio, fechaFin, 60);

            var salesOverTime = await reportRepository.GetVentasPorDiaAsync(
                KioscoId,
                fechaInicioFinal,
                fechaFinFinal,
                cancellationToken);

            if (salesOverTime.Count == 0)
            {
                return Ok(new List<VentasPorDiaDto>());
            }

            return Ok(salesOverTime);
        }

        [HttpGet("rentabilidad-categorias")]
        public async Task<ActionResult<IReadOnlyList<CategoriasRentabilidadDto>>> GetCategoriasRentabilidad(
            CancellationToken cancellationToken,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            var (fechaInicioFinal, fechaFinFinal) = NormalizarFechas(fechaInicio, fechaFin, 90);

            var categorias = await reportRepository.GetCategoriasRentabilidadAsync(KioscoId, fechaInicioFinal, fechaFinFinal, cancellationToken);

            if (categorias.Count == 0)
            {
                return Ok(new List<CategoriasRentabilidadDto>());
            }

            return Ok(categorias);
        }

        [HttpGet("ventas-chart")]
        public async Task<ActionResult<IReadOnlyList<VentaChartDto>>> GetVentasParaChart(
            CancellationToken cancellationToken,
            [FromQuery] DateTime? fecha = null)
        {
            var ventasChart = await ventaRepository.GetVentasIndividualesDelDiaAsync(KioscoId, cancellationToken, fecha);
            return Ok(ventasChart);
        }

        private static (DateTime fechaInicio, DateTime fechaFin) NormalizarFechas(
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int defaultDaysBack = 30)
        {
            var fechaInicioFinal = fechaInicio ?? DateTime.UtcNow.AddDays(-defaultDaysBack);
            var fechaFinFinal = fechaFin ?? DateTime.UtcNow;

            return (fechaInicioFinal, fechaFinFinal);
        }
    }
}
