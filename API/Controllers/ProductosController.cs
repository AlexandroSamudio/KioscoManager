using API.DTOs;
using API.Enums;
using API.Helpers;
using API.Interfaces;
using API.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
    [Authorize]
    public class ProductosController(IProductoRepository productoRepository) : BaseApiController
    {
        protected int KioscoId => User.GetKioscoId();

        /// <summary>
        /// Obtiene una lista paginada de productos
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <param name="pageNumber">Número de página (por defecto: 1, máximo: 100)</param>
        /// <param name="pageSize">Tamaño de página (por defecto: 10, máximo: 10)</param>
        /// <param name="categoriaId">ID de categoría para filtrar (0 = todas las categorías)</param>
        /// <param name="stockStatus">Estado del stock: 'low' (bajo), 'out' (agotado), 'in' (con stock), 'all' (todos) o null (todos)</param>
        /// <param name="searchTerm">Término de búsqueda para filtrar</param>
        /// <param name="sortColumn">Columna por la cual ordenar</param>
        /// <param name="sortDirection">Dirección del ordenamiento: 'asc' o 'desc'</param>
        /// <returns>Lista paginada de productos</returns>
        /// <response code="200">Lista de productos obtenida exitosamente</response>
        /// <response code="400">Parámetros de consulta inválidos</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedList<ProductoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedList<ProductoDto>>> GetProductosPaginated(
            CancellationToken cancellationToken,
            [FromQuery, Range(1, 100)] int pageNumber = 1,
            [FromQuery, Range(1, 10)] int pageSize = 10,
            int categoriaId = 0,
            StockStatus? stockStatus = null,
            string? searchTerm = null,
            string? sortColumn = null,
            string? sortDirection = null
            )
        {

            var result = await productoRepository.GetProductosPaginatedAsync(
                cancellationToken,
                KioscoId,
                pageNumber,
                pageSize,
                categoriaId == 0 ? null : categoriaId,
                stockStatus,
                searchTerm,
                sortColumn,
                sortDirection);

            if (result.IsSuccess)
            {
                Response.AddPaginationHeader(result.Data!);
                return Ok(result.Data);
            }

            return result.ToActionResult();
        }

        /// <summary>
        /// Obtiene un producto específico por su ID
        /// </summary>
        /// <param name="id">ID del producto a obtener</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Datos del producto solicitado</returns>
        /// <response code="200">Producto encontrado exitosamente</response>
        /// <response code="404">Producto no encontrado</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProductoDto>> GetProducto(int id, CancellationToken cancellationToken)
        {
            var result = await productoRepository.GetProductoByIdAsync(KioscoId, id, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Obtiene los productos con menor stock disponible
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <param name="cantidad">Cantidad de productos a retornar (por defecto: 3)</param>
        /// <returns>Lista de productos con menor stock</returns>
        /// <response code="200">Lista de productos con menor stock obtenida exitosamente</response>
        /// <response code="400">Parámetros inválidos</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        [HttpGet("low-stock")]
        [ProducesResponseType(typeof(IReadOnlyList<ProductoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IReadOnlyList<ProductoDto>>> GetProductosByLowestStock(CancellationToken cancellationToken, [FromQuery, Range(1, 10, ErrorMessage = "La cantidad debe estar entre 1 y 10.")] int cantidad = 3)
        {
            var result = await productoRepository.GetProductosByLowestStockAsync(cantidad, KioscoId, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Crea un nuevo producto
        /// </summary>
        /// <param name="dto">Datos del producto a crear</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Producto creado</returns>
        /// <response code="201">Producto creado exitosamente</response>
        /// <response code="400">Datos de entrada inválidos</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="404">Categoría no encontrada</response>
        /// <response code="409">Conflicto. SKU duplicado</response>
        [HttpPost]
        [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ProductoDto>> CreateProducto(ProductoCreateDto dto, CancellationToken cancellationToken)
        {
            var result = await productoRepository.CreateProductoAsync(KioscoId, dto, cancellationToken);
            return result.ToActionResult(producto => CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto));
        }

        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        /// <param name="id">ID del producto a actualizar</param>
        /// <param name="updateDto">Datos actualizados del producto</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación de actualización</returns>
        /// <response code="204">Producto actualizado exitosamente</response>
        /// <response code="400">Datos de entrada inválidos</response>
        /// <response code="404">Producto no encontrado</response>
        /// <response code="404">Categoría no encontrada</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="409">Conflicto. SKU duplicado</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateProducto(int id, ProductoUpdateDto updateDto, CancellationToken cancellationToken)
        {
            var result = await productoRepository.UpdateProductoAsync(KioscoId, id, updateDto, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Elimina un producto existente
        /// </summary>
        /// <param name="id">ID del producto a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación de eliminación</returns>
        /// <response code="204">Producto eliminado exitosamente</response>
        /// <response code="404">Producto no encontrado</response>
        /// <response code="400">No se puede eliminar el producto (puede tener ventas asociadas)</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteProducto(int id, CancellationToken cancellationToken)
        {
            var result = await productoRepository.DeleteProductoAsync(KioscoId, id, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Obtiene un producto por su código SKU
        /// </summary>
        /// <param name="sku">Código SKU del producto a buscar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Datos del producto con el SKU especificado</returns>
        /// <response code="200">Producto encontrado exitosamente</response>
        /// <response code="404">Producto con el SKU especificado no encontrado</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        [HttpGet("by-sku/{sku}")]
        [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProductoDto>> GetProductoBySku(string sku, CancellationToken cancellationToken)
        {
            var result = await productoRepository.GetProductoBySkuAsync(KioscoId, sku, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Obtiene el capital total invertido en inventario
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Monto total del capital invertido en productos</returns>
        /// <response code="200">Capital invertido calculado exitosamente</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        [HttpGet("capital-invertido")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<decimal>> GetCapitalInvertido(CancellationToken cancellationToken)
        {
            var result = await productoRepository.GetCapitalInvertidoTotalAsync(KioscoId, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Obtiene el total de productos únicos registrados
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número total de productos únicos en el inventario</returns>
        /// <response code="200">Total de productos calculado exitosamente</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        [HttpGet("total")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<int>> GetTotalProductos(CancellationToken cancellationToken)
        {
            var result = await productoRepository.GetTotalProductosUnicosAsync(KioscoId, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Exporta productos para descarga (solo administradores)
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <param name="limite">Límite máximo de productos a exportar (entre 1 y 5000, por defecto: 5000)</param>
        /// <returns>Flujo de datos de productos para exportación</returns>
        /// <response code="200">Datos de productos para exportación obtenidos exitosamente</response>
        /// <response code="400">Límite inválido (debe estar entre 1 y 5000)</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="403">Prohibido - Se requieren permisos de administrador</response>
        [HttpGet("export")]
        [Authorize(Policy = "RequireAdminRole")]
        [ProducesResponseType(typeof(IAsyncEnumerable<ProductoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status403Forbidden)]
        public ActionResult<IAsyncEnumerable<ProductoDto>> GetProductosForExport(
            CancellationToken cancellationToken,
            [FromQuery, Range(1, 5000, ErrorMessage = "El límite debe estar entre 1 y 5000.")] int? limite = null)
        {
            var result = productoRepository.GetProductosForExport(KioscoId, cancellationToken, limite);
            return result.ToActionResult();
        }
    }
}