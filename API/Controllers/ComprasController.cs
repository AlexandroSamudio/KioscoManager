using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ComprasController : BaseApiController
    {
        protected int KioscoId => User.GetKioscoId();
        private readonly ICompraRepository _compraRepository;

        public ComprasController(ICompraRepository compraRepository)
        {
            _compraRepository = compraRepository;
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<CompraDto>>> GetCompras(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortDirection = null)
        {

            var compras = await _compraRepository.GetComprasAsync(
                KioscoId, pageNumber, pageSize, HttpContext.RequestAborted,
                fechaDesde, fechaHasta, sortColumn, sortDirection);

            Response.AddPaginationHeader(compras);

            return Ok(compras);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CompraDto>> GetCompra(int id)
        {
            var compra = await _compraRepository.GetCompraByIdAsync(KioscoId, id, HttpContext.RequestAborted);

            if (compra == null) return NotFound("Compra no encontrada");

            return compra;
        }

        [HttpGet("recientes")]
        public async Task<ActionResult<IReadOnlyList<CompraDto>>> GetComprasRecientes([FromQuery] int cantidad = 5)
        {
            var compras = await _compraRepository.GetComprasRecientesAsync(KioscoId, cantidad, HttpContext.RequestAborted);

            return Ok(compras);
        }

        [HttpGet("total")]
        public async Task<ActionResult<decimal>> GetTotalComprasPeriodo(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            var total = await _compraRepository.GetTotalComprasDelPeriodoAsync(
                KioscoId, HttpContext.RequestAborted, fechaDesde, fechaHasta);

            return Ok(total);
        }

        [HttpPost]
        public async Task<ActionResult<CompraDto>> CreateCompra(CompraCreateDto compraData)
        {
            var usuarioId = User.GetUserId();

            try
            {
                if (compraData.Productos.Count == 0)
                {
                    return BadRequest("La compra debe contener al menos un producto");
                }

                var compraDto = await _compraRepository.CreateCompraAsync(
                    compraData, KioscoId, usuarioId, HttpContext.RequestAborted);

                return CreatedAtAction(nameof(GetCompra), new { id = compraDto.Id }, compraDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno al procesar la compra. Por favor, inténtelo de nuevo más tarde.");
            }
        }
    }
}
