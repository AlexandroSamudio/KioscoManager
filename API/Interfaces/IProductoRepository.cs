using API.DTOs;
using API.Helpers;

namespace API.Interfaces
{
    public interface IProductoRepository
    {
        Task<PagedList<ProductoDto>> GetProductosAsync(int kioscoId, int pageNumber, int pageSize, CancellationToken cancellationToken, int? categoriaId = null, string? stockStatus = null, string? searchTerm = null, string? sortColumn = null, string? sortDirection = null);
        Task<ProductoDto?> GetProductoByIdAsync(int kioscoId, int id, CancellationToken cancellationToken);
        Task<IReadOnlyList<ProductoDto>> GetProductosByLowestStockAsync(int cantidad, int kioscoId, CancellationToken cancellationToken);
        Task<bool> DeleteProductoAsync(int kioscoId, int id, CancellationToken cancellationToken);
        Task<ProductoDto?> CreateProductoAsync(int kioscoId, ProductoCreateDto dto, CancellationToken cancellationToken);
        Task<ProductoDto?> UpdateProductoAsync(int kioscoId, int id, ProductoCreateDto dto, CancellationToken cancellationToken);

    }
}
