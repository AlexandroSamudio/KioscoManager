using API.DTOs;
using API.Interfaces;
using API.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ProductosController(IProductoRepository productoRepository) : BaseApiController
    {
        protected int KioscoId => User.GetKioscoId();

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductoDto>>> GetProductos(CancellationToken cancellationToken)
        {
            var productos = await productoRepository.GetProductosAsync(KioscoId, cancellationToken);
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
            var productos = await productoRepository.GetProductosByLowestStockAsync(cantidad, KioscoId, cancellationToken);
            return Ok(productos);
        }
    }
}
