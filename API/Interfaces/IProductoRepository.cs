using API.DTOs;

namespace API.Interfaces
{
    public interface IProductoRepository
    {
        Task<IEnumerable<ProductoDto>> GetProductosAsync();
        Task<ProductoDto?> GetProductoByIdAsync(int id);
        Task<IEnumerable<ProductoDto>> GetProductosByLowestStockAsync();
    }
}
