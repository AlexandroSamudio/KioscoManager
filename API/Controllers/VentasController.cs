using API.DTOs;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class VentasController(IVentaRepository ventaRepository) : BaseApiController
    {
        private readonly IVentaRepository _ventaRepository = ventaRepository;

        protected int KioscoId => User.GetKioscoId();
        protected int UserId => User.GetUserId();

        [HttpGet("dia")]
        public async Task<ActionResult<IReadOnlyList<VentaDto>>> GetVentasDelDia(CancellationToken cancellationToken)
        {
            var ventas = await _ventaRepository.GetVentasDelDiaAsync(KioscoId, cancellationToken, null);
            return Ok(ventas);
        }

        [HttpGet("dia/{fecha:datetime}")]
        public async Task<ActionResult<IReadOnlyList<VentaDto>>> GetVentasDelDia(DateTime fecha, CancellationToken cancellationToken)
        {
            var ventas = await _ventaRepository.GetVentasDelDiaAsync(KioscoId, cancellationToken, fecha);
            return Ok(ventas);
        }

        [HttpGet("recientes")]
        public async Task<ActionResult<IReadOnlyList<VentaDto>>> GetVentasRecientes(CancellationToken cancellationToken, [FromQuery] int cantidad = 4)
        {
            if (cantidad <= 0)
            {
                return BadRequest("La cantidad debe ser mayor que cero.");
            }

            var ventas = await _ventaRepository.GetVentasRecientesAsync(KioscoId, cantidad, cancellationToken);
            return Ok(ventas);
        }

        [HttpGet("total-dia")]
        public async Task<ActionResult<decimal>> GetTotalVentasDelDia(CancellationToken cancellationToken)
        {
            var total = await _ventaRepository.GetTotalVentasDelDiaAsync(KioscoId, cancellationToken, null);
            return Ok(total);
        }

        [HttpGet("total-dia/{fecha:datetime}")]
        public async Task<ActionResult<decimal>> GetTotalVentasDelDia(DateTime fecha, CancellationToken cancellationToken)
        {
            var total = await _ventaRepository.GetTotalVentasDelDiaAsync(KioscoId, cancellationToken, fecha);
            return Ok(total);
        }

        [HttpGet("productos-mas-vendidos")]
        public async Task<ActionResult<IReadOnlyList<ProductoMasVendidoDto>>> GetProductosMasVendidosDelDia(CancellationToken cancellationToken, [FromQuery] int cantidad = 4)
        {
            if (cantidad <= 0)
            {
                return BadRequest("La cantidad debe ser mayor que cero.");
            }

            var productos = await _ventaRepository.GetProductosMasVendidosDelDiaAsync(KioscoId, cantidad, cancellationToken, null);
            return Ok(productos);
        }
        
        [HttpGet("productos-mas-vendidos/{fecha:datetime}")]
        public async Task<ActionResult<IReadOnlyList<ProductoMasVendidoDto>>> GetProductosMasVendidosDelDia(DateTime fecha, CancellationToken cancellationToken, [FromQuery] int cantidad = 4)
        {
            var productos = await _ventaRepository.GetProductosMasVendidosDelDiaAsync(KioscoId, cantidad, cancellationToken, fecha);
            return Ok(productos);
        }

        [HttpPost("finalizar")]
        public async Task<ActionResult<VentaDto>> FinalizarVenta(VentaCreateDto ventaDto, CancellationToken cancellationToken)
        {
            if (ventaDto.Productos == null || !ventaDto.Productos.Any())
            {
                return BadRequest("La venta debe contener al menos un producto");
            }

            var result = await _ventaRepository.CreateVentaAsync(ventaDto, KioscoId, UserId, cancellationToken);
            
            return result.ToActionResult(venta => CreatedAtAction(nameof(GetVentasDelDia), new { fecha = venta.Fecha.Date }, venta));
        }
    }
}
