using API.DTOs;
using API.Helpers;

namespace API.Interfaces
{
    public interface IProductoRepository
    {
        Task<PagedList<ProductoDto>> GetProductosAsync(int kioscoId, int pageNumber, int pageSize, CancellationToken cancellationToken, int? categoriaId = null, string? stockStatus = null);
        Task<ProductoDto?> GetProductoByIdAsync(int kioscoId, int id, CancellationToken cancellationToken);
        Task<IReadOnlyList<ProductoDto>> GetProductosByLowestStockAsync(int cantidad, int kioscoId, CancellationToken cancellationToken);

    }
}
