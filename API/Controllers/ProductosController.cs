using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class ProductosController(IProductoRepository productoRepository) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos(CancellationToken cancellationToken)
        {
            var productos = await productoRepository.GetProductosAsync()
                .ToListAsync(cancellationToken);
            return Ok(productos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDto>> GetProducto(int id)
        {
            var producto = await productoRepository.GetProductoByIdAsync(id);

            if (producto == null)
            {
                return NotFound();
            }

            return Ok(producto);
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductosByLowestStock(CancellationToken cancellationToken)
        {
            var productos = await productoRepository.GetProductosByLowestStockAsync()
                .ToListAsync(cancellationToken);
            return Ok(productos);
        }
    }
}
