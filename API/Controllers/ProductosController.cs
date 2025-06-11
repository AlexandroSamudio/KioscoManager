using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ProductosController(IProductoRepository productoRepository) : BaseApiController
    {
        private readonly IProductoRepository _productoRepository = productoRepository;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos()
        {
            var productos = await _productoRepository.GetProductosAsync();
            return Ok(productos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDto>> GetProducto(int id)
        {
            var producto = await _productoRepository.GetProductoByIdAsync(id);

            if (producto == null)
            {
                return NotFound();
            }

            return Ok(producto);
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductosByLowestStock()
        {
            var productos = await _productoRepository.GetProductosByLowestStockAsync();
            if (productos == null || !productos.Any())
            {
                return NotFound("No hay productos con bajo stock.");
            }
            return Ok(productos);
            
        }
    }
}
