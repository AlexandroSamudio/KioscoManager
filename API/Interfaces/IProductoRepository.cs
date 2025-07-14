using API.DTOs;
using API.Helpers;

namespace API.Interfaces
{
    public interface IProductoRepository
    {
        Task<PagedList<ProductoDto>> GetProductosAsync(CancellationToken cancellationToken,int kioscoId, int pageNumber, int pageSize, int? categoriaId = null, string? stockStatus = null, string? searchTerm = null, string? sortColumn = null, string? sortDirection = null);
        Task<ProductoDto?> GetProductoByIdAsync(int kioscoId, int id, CancellationToken cancellationToken);
        Task<IReadOnlyList<ProductoDto>> GetProductosByLowestStockAsync(int cantidad, int kioscoId, CancellationToken cancellationToken);
        Task<Result> DeleteProductoAsync(int kioscoId, int id, CancellationToken cancellationToken);
        Task<Result<ProductoDto>> CreateProductoAsync(int kioscoId, ProductoCreateDto dto, CancellationToken cancellationToken);
        Task<Result> UpdateProductoAsync(int kioscoId, int id, ProductoUpdateDto dto, CancellationToken cancellationToken);
        Task<ProductoDto?> GetProductoBySkuAsync(int kioscoId, string sku, CancellationToken cancellationToken);
        Task<ProductoInfoDto> GetCapitalInvertidoTotalAsync(int kioscoId, CancellationToken cancellationToken);
        Task<int> GetTotalProductosUnicosAsync(int kioscoId, CancellationToken cancellationToken);
    }
}
    