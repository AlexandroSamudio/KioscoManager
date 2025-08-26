using API.DTOs;
using API.Entities;

namespace API.Interfaces
{
    public interface ICompraRepository
    {
        Task<CompraDto?> GetCompraByIdAsync(int kioscoId, int id, CancellationToken cancellationToken);
        IQueryable<Compra> GetComprasQueryable(int kioscoId);
        IAsyncEnumerable<Compra> GetComprasAsync(int kioscoId, CancellationToken cancellationToken);
        Task<Dictionary<int, Producto>> GetProductosByIdsAsync(int kioscoId, IEnumerable<int> productosIds, CancellationToken cancellationToken);
        Task<Result<CompraDto>> CreateCompraWithStockAdjustmentsAsync(CompraCreateDto compraData, int kioscoId, int usuarioId, CancellationToken cancellationToken);
    }
}