using API.DTOs;

namespace API.Interfaces
{
    public interface IProductoRepository
    {
        Task<IReadOnlyList<ProductoDto>> GetProductosAsync(int kioscoId,CancellationToken cancellationToken) ;
        Task<ProductoDto?> GetProductoByIdAsync(int kioscoId,int id,CancellationToken cancellationToken);
        Task<IReadOnlyList<ProductoDto>> GetProductosByLowestStockAsync(int cantidad,int kioscoId,CancellationToken cancellationToken);
    }
}
