using API.DTOs;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ReportesController : BaseApiController
    {
        private readonly IReporteRepository _reportRepository;

        public ReportesController(IReporteRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        protected int KioscoId => User.GetKioscoId();

        [HttpGet]
        public async Task<ActionResult<ReporteDto>> GetReporte(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            var (isValid, errorMessage) = ConfigurarFechasReporte(fechaInicio, fechaFin, out var start, out var end);
            if (!isValid)
            {
                return BadRequest(errorMessage);
            }

            var summary = await _reportRepository.CalculateKpiReporteAsync(KioscoId, start, end);
            return Ok(summary);
        }

        [HttpGet("top-productos")]
        public async Task<ActionResult<IReadOnlyList<ProductoMasVendidoDto>>> GetTopProductos(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int limit = 5)
        {
            var (isValid, errorMessage) = ConfigurarFechasReporte(fechaInicio, fechaFin, out var start, out var end);
            if (!isValid)
            {
                return BadRequest(errorMessage);
            }

            limit = Math.Clamp(limit, 1, 50);
            pageSize = Math.Clamp(pageSize, 1, 10);
            pageNumber = Math.Max(pageNumber, 1);

            var topProducts = await _reportRepository.GetTopProductsByVentasAsync(
                KioscoId,
                pageNumber,
                pageSize,
                start,
                end,
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
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            var (isValid, errorMessage) = ConfigurarFechasReporte(fechaInicio, fechaFin, out var start, out var end);
            if (!isValid)
            {
                return BadRequest(errorMessage);
            }

            var salesOverTime = await _reportRepository.GetVentasPorDiaAsync(
                KioscoId,
                start,
                end);

            if (salesOverTime.Count == 0)
            {
                return Ok(new List<VentasPorDiaDto>());
            }

            return Ok(salesOverTime);
        }

        [HttpGet("rentabilidad-categorias")]
        public async Task<ActionResult<IReadOnlyList<CategoriasRentabilidadDto>>> GetCategoriasRentabilidad(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            var (isValid, errorMessage) = ConfigurarFechasReporte(fechaInicio, fechaFin, out var start, out var end);
            if (!isValid)
            {
                return BadRequest(errorMessage);
            }

            var categorias = await _reportRepository.GetCategoriasRentabilidadAsync(KioscoId, start, end);

            if (categorias.Count == 0)
            {
                return Ok(new List<CategoriasRentabilidadDto>());
            }

            return Ok(categorias);
        }

        private static (bool isValid, string? errorMessage) ConfigurarFechasReporte(
            DateTime? fechaInicio,
            DateTime? fechaFin,
            out DateTime start,
            out DateTime end)
        {
            start = fechaInicio.HasValue
                ? DateTime.SpecifyKind(fechaInicio.Value, DateTimeKind.Utc)
                : new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            end = fechaFin.HasValue
                ? DateTime.SpecifyKind(fechaFin.Value, DateTimeKind.Utc)
                : DateTime.UtcNow;

            if (start > end)
            {
                return (false, "La fecha de inicio debe ser anterior o igual a la fecha final");
            }

            var maxRange = TimeSpan.FromDays(366);
            if (end - start > maxRange)
            {
                return (false, $"El rango de fechas no puede superar {maxRange.Days} días");
            }

            return (true, null);
        }
    }
}
