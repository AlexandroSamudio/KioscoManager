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
            CancellationToken cancellationToken = default,
            int categoriaId = 0,
            string? stockStatus = null,
            string? searchTerm = null)
        {
            pageSize = Math.Clamp(pageSize, 1, 10);

            if (!string.IsNullOrWhiteSpace(stockStatus) && stockStatus is not ("low" or "out" or "in"))
            {
                return BadRequest("Valor de stockStatus no válido. Debe ser 'low', 'out' o 'in'.");
            }

            var productos = await productoRepository.GetProductosAsync(
                KioscoId,
                pageNumber,
                pageSize,
                cancellationToken,
                categoriaId == 0 ? null : categoriaId,
                stockStatus,
                searchTerm);
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
            var productos = await productoRepository.GetProductosByLowestStockAsync(cantidad, KioscoId, cancellationToken);
            return Ok(productos);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id, CancellationToken cancellationToken)
        {
            var deleted = await productoRepository.DeleteProductoAsync(KioscoId, id, cancellationToken);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<ProductoDto>> CreateProducto([FromBody] ProductoCreateDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await productoRepository.CreateProductoAsync(KioscoId, dto, cancellationToken);
            if (result == null)
            {
                return Conflict("El SKU ya existe para este kiosco.");
            }
            return CreatedAtAction(nameof(GetProducto), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductoDto>> UpdateProducto(int id, [FromBody] ProductoCreateDto dto, CancellationToken cancellationToken)
        {
            var exists = await productoRepository.GetProductoByIdAsync(KioscoId, id, cancellationToken);
            if (exists == null)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await productoRepository.UpdateProductoAsync(KioscoId, id, dto, cancellationToken);
            if (updated == null)
            {
                return Conflict("El SKU ya existe para otro producto en este kiosco.");
            }
            return Ok(updated);
        }
    }
}
