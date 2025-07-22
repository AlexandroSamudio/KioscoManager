using API.DTOs;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ComprasController(ICompraRepository _compraRepository) : BaseApiController
    {
        protected int KioscoId => User.GetKioscoId();

        [HttpGet("{id}")]
        public async Task<ActionResult<CompraDto>> GetCompra(int id, CancellationToken cancellationToken)
        {
            var compra = await _compraRepository.GetCompraByIdAsync(KioscoId, id, cancellationToken);

            if (compra == null) return NotFound("Compra no encontrada");

            return compra;
        }

        [HttpPost]
        public async Task<ActionResult<CompraDto>> CreateCompra(CompraCreateDto compraDto, CancellationToken cancellationToken)
        {
            var usuarioId = User.GetUserId();
            if (compraDto.Detalles.Count == 0)
            {
                return BadRequest("La compra debe contener al menos un producto");
            }

            var result = await _compraRepository.CreateCompraAsync(
                compraDto, KioscoId, usuarioId, cancellationToken);

            return result.ToActionResult(compra => CreatedAtAction(nameof(GetCompra), new { id = compra.Id }, compra));
        }

        [HttpGet("export")]
        public async Task<ActionResult<IReadOnlyList<CompraDto>>> GetComprasForExport(
            CancellationToken cancellationToken,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int? limite = null)
        {
            if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio > fechaFin)
            {
                return BadRequest("La fecha inicio no puede ser mayor que la fecha fin");
            }

            if (limite.HasValue && limite <= 0)
            {
                return BadRequest("El límite debe ser mayor que cero");
            }

            if (limite.HasValue && limite > 10000)
            {
                return BadRequest("El límite no puede ser mayor a 10,000 registros");
            }

            var compras = await _compraRepository.GetComprasForExportAsync(KioscoId, cancellationToken, fechaInicio, fechaFin, limite);
            return Ok(compras);
        }
    }
}
