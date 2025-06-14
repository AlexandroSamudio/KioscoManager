using API.DTOs;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class VentasController(IVentaRepository ventaRepository) : BaseApiController
    {
        [HttpGet("dia")]
        public async Task<ActionResult<IEnumerable<VentaDto>>> GetVentasDelDia(CancellationToken cancellationToken)
        {
            var kioscoId = User.GetKioscoId();
            var ventas = await ventaRepository.GetVentasDelDiaAsync(kioscoId, cancellationToken);
            return Ok(ventas);
        }

        [HttpGet("dia/{fecha:datetime}")]
        public async Task<ActionResult<IEnumerable<VentaDto>>> GetVentasDelDia(DateTime fecha, CancellationToken cancellationToken)
        {
            var kioscoId = User.GetKioscoId();
            var ventas = await ventaRepository.GetVentasDelDiaAsync(kioscoId, fecha, cancellationToken);
            return Ok(ventas);
        }

        [HttpGet("recientes")]
        public async Task<ActionResult<IEnumerable<VentaDto>>> GetVentasRecientes([FromQuery] int cantidad = 4, CancellationToken cancellationToken = default)
        {
            var kioscoId = User.GetKioscoId();
            var ventas = await ventaRepository.GetVentasRecientesAsync(kioscoId, cantidad, cancellationToken);
            return Ok(ventas);
        }

        [HttpGet("total-dia")]
        public async Task<ActionResult<decimal>> GetTotalVentasDelDia(CancellationToken cancellationToken = default)
        {
            var kioscoId = User.GetKioscoId();
            var total = await ventaRepository.GetTotalVentasDelDiaAsync(kioscoId, cancellationToken);
            return Ok(total);
        }

        [HttpGet("total-dia/{fecha:datetime}")]
        public async Task<ActionResult<decimal>> GetTotalVentasDelDia(DateTime fecha, CancellationToken cancellationToken = default)
        {
            var kioscoId = User.GetKioscoId();
            var total = await ventaRepository.GetTotalVentasDelDiaAsync(kioscoId, fecha, cancellationToken);
            return Ok(total);
        }

        [HttpGet("productos-mas-vendidos")]
        public async Task<ActionResult<IEnumerable<ProductoMasVendidoDto>>> GetProductosMasVendidosDelDia([FromQuery] int cantidad = 4, CancellationToken cancellationToken = default)
        {
            var kioscoId = User.GetKioscoId();
            var productos = await ventaRepository.GetProductosMasVendidosDelDiaAsync(kioscoId, cantidad, cancellationToken);
            return Ok(productos);
        }
        
        [HttpGet("productos-mas-vendidos/{fecha:datetime}")]
        public async Task<ActionResult<IEnumerable<ProductoMasVendidoDto>>> GetProductosMasVendidosDelDia(DateTime fecha, [FromQuery] int cantidad = 4, CancellationToken cancellationToken = default)
        {
            var kioscoId = User.GetKioscoId();
            var productos = await ventaRepository.GetProductosMasVendidosDelDiaAsync(kioscoId, fecha, cantidad, cancellationToken);
            return Ok(productos);
        }
    }
}
