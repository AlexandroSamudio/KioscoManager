using API.DTOs;

namespace API.Interfaces
{
    public interface IProductoRepository
    {
        IAsyncEnumerable<ProductoDto> GetProductosAsync();
        Task<ProductoDto?> GetProductoByIdAsync(int id);
        IAsyncEnumerable<ProductoDto> GetProductosByLowestStockAsync();
    }
}
