using API.DTOs;

namespace API.Interfaces
{
    public interface IProductoRepository
    {
        Task<IReadOnlyList<ProductoDto>> GetProductosAsync(int kioscoId,CancellationToken cancellationToken = default);
        Task<ProductoDto?> GetProductoByIdAsync(int kioscoId,int id,CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ProductoDto>> GetProductosByLowestStockAsync(int kioscoId,CancellationToken cancellationToken = default);
    }
}
