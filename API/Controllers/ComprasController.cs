using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ComprasController(ICompraRepository compraRepository) : BaseApiController
    {
        protected int KioscoId => User.GetKioscoId();
        private readonly ICompraRepository _compraRepository = compraRepository;

        [HttpGet]
        public async Task<ActionResult<PagedList<CompraDto>>> GetCompras(
            CancellationToken cancellationToken,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortDirection = null)
        {
            pageSize = Math.Clamp(pageSize, 1, 10);
            pageNumber = Math.Max(pageNumber, 1);

            var compras = await _compraRepository.GetComprasAsync(
                KioscoId, pageNumber, pageSize, cancellationToken,
                fechaDesde, fechaHasta, sortColumn, sortDirection);

            Response.AddPaginationHeader(compras);

            return Ok(compras);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CompraDto>> GetCompra(int id,CancellationToken cancellationToken)
        {
            var compra = await _compraRepository.GetCompraByIdAsync(KioscoId, id, cancellationToken);

            if (compra == null) return NotFound("Compra no encontrada");

            return compra;
        }

        [HttpGet("recientes")]
        public async Task<ActionResult<IReadOnlyList<CompraDto>>> GetComprasRecientes(CancellationToken cancellationToken,[FromQuery] int cantidad = 5)
        {
            if (cantidad < 1 || cantidad > 50) return BadRequest("La cantidad debe estar entre 1 y 50");
            var compras = await _compraRepository.GetComprasRecientesAsync(KioscoId, cantidad, cancellationToken);

            return Ok(compras);
        }

        [HttpGet("total")]
        public async Task<ActionResult<decimal>> GetTotalComprasPeriodo(
            CancellationToken cancellationToken,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            if (fechaDesde.HasValue && fechaHasta.HasValue && fechaDesde > fechaHasta)
            {
                return BadRequest("La fecha desde no puede ser mayor que la fecha hasta");
            }

            var total = await _compraRepository.GetTotalComprasDelPeriodoAsync(KioscoId, cancellationToken, fechaDesde, fechaHasta);

            return Ok(total);
        }

        [HttpPost]
        public async Task<ActionResult<CompraDto>> CreateCompra(CompraCreateDto compraDto,CancellationToken cancellationToken)
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
    }
}
