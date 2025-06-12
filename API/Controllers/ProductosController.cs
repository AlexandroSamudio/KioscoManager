using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ProductosController(IProductoRepository productoRepository) : BaseApiController
    {
        [HttpGet]
        public ActionResult<IAsyncEnumerable<ProductoDto>> GetProductos()
        {
            var productos = productoRepository.GetProductosAsync();
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
        public ActionResult<IAsyncEnumerable<ProductoDto>> GetProductosByLowestStock()
        {
            var productos = productoRepository.GetProductosByLowestStockAsync();
            return Ok(productos);
            
        }
    }
}
