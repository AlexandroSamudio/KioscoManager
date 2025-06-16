using API.DTOs;
using API.Helpers;
using API.Interfaces;
using API.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ProductosController(IProductoRepository productoRepository) : BaseApiController
    {
        protected int KioscoId => User.GetKioscoId();

        [HttpGet]
        public async Task<ActionResult<PagedList<ProductoDto>>> GetProductos(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            CancellationToken cancellationToken = default)
        {
            pageSize = Math.Clamp(pageSize, 1, 10);
            var productos = await productoRepository.GetProductosAsync(KioscoId, pageNumber, pageSize, cancellationToken);
            Response.AddPaginationHeader(productos);
            return Ok(productos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDto>> GetProducto(int id, CancellationToken cancellationToken)
        {
            var producto = await productoRepository.GetProductoByIdAsync(KioscoId, id, cancellationToken);

            if (producto == null)
            {
                return NotFound();
            }

            return Ok(producto);
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<IReadOnlyList<ProductoDto>>> GetProductosByLowestStock(CancellationToken cancellationToken, [FromQuery] int cantidad = 3)
        {
            var productos = await productoRepository.GetProductosByLowestStockAsync(KioscoId,cantidad, cancellationToken);
            return Ok(productos);
        }
    }
}
