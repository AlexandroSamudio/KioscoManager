using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class ProductosController(IProductoRepository productoRepository) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos(CancellationToken ct)
        {
            var productos = new List<ProductoDto>();
            await foreach (var producto in productoRepository.GetProductosAsync().WithCancellation(ct))
            {
                productos.Add(producto);
            }
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
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductosByLowestStock(CancellationToken ct)
        {
            var productos = new List<ProductoDto>();
            await foreach (var producto in productoRepository.GetProductosByLowestStockAsync().WithCancellation(ct))
            {
                productos.Add(producto);
            }
            return Ok(productos);
        }
    }
}
