using API.DTOs;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class VentasController(IVentaRepository ventaRepository) : BaseApiController
    {
        protected int KioscoId => User.GetKioscoId();
        protected int UserId => User.GetUserId();

        [HttpGet("dia")]
        public async Task<ActionResult<IReadOnlyList<VentaDto>>> GetVentasDelDia(CancellationToken cancellationToken)
        {
            var ventas = await ventaRepository.GetVentasDelDiaAsync(KioscoId, cancellationToken, null);
            return Ok(ventas);
        }

        [HttpGet("dia/{fecha:datetime}")]
        public async Task<ActionResult<IReadOnlyList<VentaDto>>> GetVentasDelDia(DateTime fecha, CancellationToken cancellationToken)
        {
            var ventas = await ventaRepository.GetVentasDelDiaAsync(KioscoId, cancellationToken, fecha);
            return Ok(ventas);
        }

        [HttpGet("recientes")]
        public async Task<ActionResult<IReadOnlyList<VentaDto>>> GetVentasRecientes(CancellationToken cancellationToken, [FromQuery] int cantidad = 4)
        {
            if (cantidad <= 0)
            {
                return BadRequest("La cantidad debe ser mayor que cero.");
            }

            var ventas = await ventaRepository.GetVentasRecientesAsync(KioscoId, cantidad, cancellationToken);
            return Ok(ventas);
        }

        [HttpGet("total-dia")]
        public async Task<ActionResult<decimal>> GetTotalVentasDelDia(CancellationToken cancellationToken)
        {
            var total = await ventaRepository.GetTotalVentasDelDiaAsync(KioscoId, cancellationToken, null);
            return Ok(total);
        }

        [HttpGet("total-dia/{fecha:datetime}")]
        public async Task<ActionResult<decimal>> GetTotalVentasDelDia(DateTime fecha, CancellationToken cancellationToken)
        {
            var total = await ventaRepository.GetTotalVentasDelDiaAsync(KioscoId, cancellationToken, fecha);
            return Ok(total);
        }

        [HttpGet("productos-mas-vendidos")]
        public async Task<ActionResult<IReadOnlyList<ProductoMasVendidoDto>>> GetProductosMasVendidosDelDia(CancellationToken cancellationToken, [FromQuery] int cantidad = 4)
        {
            if (cantidad <= 0)
            {
                return BadRequest("La cantidad debe ser mayor que cero.");
            }

            var productos = await ventaRepository.GetProductosMasVendidosDelDiaAsync(KioscoId, cantidad, cancellationToken, null);
            return Ok(productos);
        }

        [HttpGet("productos-mas-vendidos/{fecha:datetime}")]
        public async Task<ActionResult<IReadOnlyList<ProductoMasVendidoDto>>> GetProductosMasVendidosDelDia(DateTime fecha, CancellationToken cancellationToken, [FromQuery] int cantidad = 4)
        {
            var productos = await ventaRepository.GetProductosMasVendidosDelDiaAsync(KioscoId, cantidad, cancellationToken, fecha);
            return Ok(productos);
        }

        [HttpPost("finalizar")]
        public async Task<ActionResult<VentaDto>> FinalizarVenta(VentaCreateDto ventaDto, CancellationToken cancellationToken)
        {
            if (ventaDto.Productos == null || !ventaDto.Productos.Any())
            {
                return BadRequest("La venta debe contener al menos un producto");
            }

            var result = await ventaRepository.CreateVentaAsync(ventaDto, KioscoId, UserId, cancellationToken);

            return result.ToActionResult(venta => CreatedAtAction(nameof(GetVentasDelDia), new { fecha = venta.Fecha.Date }, venta));
        }

        [HttpGet("export")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult<IReadOnlyList<VentaDto>>> GetVentasForExport(
            CancellationToken cancellationToken,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int? limite = null)
        {
            if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio > fechaFin)
            {
                return BadRequest("La fecha desde no puede ser mayor que la fecha hasta");
            }

            if (limite.HasValue && limite <= 0)
            {
                return BadRequest("El límite debe ser mayor que cero");
            }

            if (limite.HasValue && limite > 10000)
            {
                return BadRequest("El límite no puede ser mayor a 10,000 registros");
            }

            var ventas = await ventaRepository.GetVentasForExportAsync(KioscoId, cancellationToken, fechaInicio, fechaFin, limite);
            return Ok(ventas);
        }
    }
}
