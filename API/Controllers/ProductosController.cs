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
            CancellationToken cancellationToken,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            int categoriaId = 0,
            string? stockStatus = null,
            string? searchTerm = null,
            string? sortColumn = null,
            string? sortDirection = null
            )
        {
            pageSize = Math.Clamp(pageSize, 1, 10);
            pageNumber = Math.Max(pageNumber, 1);

            var productos = await productoRepository.GetProductosAsync(
                cancellationToken,
                KioscoId,
                pageNumber,
                pageSize,
                categoriaId == 0 ? null : categoriaId,
                stockStatus,
                searchTerm,
                sortColumn,
                sortDirection);
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
            var result = await productoRepository.DeleteProductoAsync(KioscoId, id, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPost]
        public async Task<ActionResult<ProductoDto>> CreateProducto(ProductoCreateDto dto, CancellationToken cancellationToken)
        {
            var result = await productoRepository.CreateProductoAsync(KioscoId, dto, cancellationToken);
            
            return result.ToActionResult(producto => CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProducto(int id, ProductoUpdateDto updateDto, CancellationToken cancellationToken)
        {
            var result = await productoRepository.UpdateProductoAsync(KioscoId, id, updateDto, cancellationToken);
            return result.ToActionResult();
        }

        [HttpGet("by-sku/{sku}")]
        public async Task<ActionResult<ProductoDto>> GetProductoBySku(string sku, CancellationToken cancellationToken)
        {
            var producto = await productoRepository.GetProductoBySkuAsync(KioscoId, sku, cancellationToken);

            if (producto == null)
            {
                return NotFound($"No se encontró un producto con SKU '{sku}' en este kiosco");
            }

            return Ok(producto);
        }

        [HttpGet("capital-invertido")]
        public async Task<ActionResult<ProductoInfoDto>> GetCapitalInvertido(CancellationToken cancellationToken)
        {
            var resultado = await productoRepository.GetCapitalInvertidoTotalAsync(KioscoId, cancellationToken);
            return Ok(resultado);
        }

        [HttpGet("total")]
        public async Task<ActionResult<int>> GetTotalProductos(CancellationToken cancellationToken)
        {
            var total = await productoRepository.GetTotalProductosUnicosAsync(KioscoId, cancellationToken);
            return Ok(new { totalProductos = total });
        }
    }
}
