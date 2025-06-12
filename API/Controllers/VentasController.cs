using API.DTOs;
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
            var ventas = await ventaRepository.GetVentasDelDiaAsync(cancellationToken)
                .ToListAsync(cancellationToken);
            return Ok(ventas);
        }

        [HttpGet("dia/{fecha:datetime}")]
        public async Task<ActionResult<IEnumerable<VentaDto>>> GetVentasDelDia(DateTime fecha, CancellationToken cancellationToken)
        {
            var ventas = await ventaRepository.GetVentasDelDiaAsync(fecha, cancellationToken)
                .ToListAsync(cancellationToken);
            return Ok(ventas);
        }

        [HttpGet("recientes")]
        public async Task<ActionResult<IEnumerable<VentaDto>>> GetVentasRecientes([FromQuery] int cantidad = 4, CancellationToken cancellationToken = default)
        {
            var ventas = await ventaRepository.GetVentasRecientesAsync(cantidad, cancellationToken)
                .ToListAsync(cancellationToken);
            return Ok(ventas);
        }
    }
}
