using API.DTOs;
using API.Interfaces;
using API.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ProductosController(IProductoRepository productoRepository) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductoDto>>> GetProductos(CancellationToken cancellationToken)
        {
            var kioscoId = User.GetKioscoId();
            var productos = await productoRepository.GetProductosAsync(kioscoId, cancellationToken);
            return Ok(productos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDto>> GetProducto(int id, CancellationToken cancellationToken)
        {
            var kioscoId = User.GetKioscoId();
            var producto = await productoRepository.GetProductoByIdAsync(kioscoId, id, cancellationToken);

            if (producto == null)
            {
                return NotFound();
            }

            return Ok(producto);
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<IReadOnlyList<ProductoDto>>> GetProductosByLowestStock(CancellationToken cancellationToken)
        {
            var kioscoId = User.GetKioscoId();
            var productos = await productoRepository.GetProductosByLowestStockAsync(kioscoId, cancellationToken);
            return Ok(productos);
        }
    }
}
