using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IProductoRepository
    {
        Task<Result<ProductoDto>> GetProductoByIdAsync(int kioscoId, int id, CancellationToken cancellationToken);
        Task<Result<PagedList<ProductoDto>>> GetProductosPaginatedAsync(CancellationToken cancellationToken, int kioscoId, int pageNumber, int pageSize, int? categoriaId = null, string? stockStatus = null, string? searchTerm = null, string? sortColumn = null, string? sortDirection = null);
        Task<Result<IReadOnlyList<ProductoDto>>> GetProductosByLowestStockAsync(int cantidad, int kioscoId, CancellationToken cancellationToken);
        Task<Result<ProductoDto>> CreateProductoAsync(int kioscoId, ProductoCreateDto dto, CancellationToken cancellationToken);
        Task<Result> UpdateProductoAsync(int kioscoId, int id, ProductoUpdateDto dto, CancellationToken cancellationToken);
        Task<Result> DeleteProductoAsync(int kioscoId, int id, CancellationToken cancellationToken);
        Task<Result<ProductoDto>> GetProductoBySkuAsync(int kioscoId, string sku, CancellationToken cancellationToken);
        Task<Result<decimal>> GetCapitalInvertidoTotalAsync(int kioscoId, CancellationToken cancellationToken);
        Task<Result<int>> GetTotalProductosUnicosAsync(int kioscoId, CancellationToken cancellationToken);
        Result<IAsyncEnumerable<ProductoDto>> GetProductosForExport(int kioscoId, CancellationToken cancellationToken, int? limite = null);
    }
}
    