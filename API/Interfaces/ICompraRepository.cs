using API.DTOs;
using API.Entities;

namespace API.Interfaces
{
    public interface ICompraRepository
    {
        Task<Result<CompraDto>> GetCompraByIdAsync(int kioscoId, int id, CancellationToken cancellationToken);
        IQueryable<Compra> GetComprasQueryable(int kioscoId);
        IAsyncEnumerable<Compra> GetComprasAsync(int kioscoId, CancellationToken cancellationToken);
        Task<Dictionary<int, Producto>> GetProductosByIdsAsync(int kioscoId, IEnumerable<int> productosIds, CancellationToken cancellationToken);
        Task<Result<CompraDto>> CreateCompraWithStockAdjustmentsAsync(CompraCreateDto compraData, int kioscoId, int usuarioId, CancellationToken cancellationToken);
        Result<IAsyncEnumerable<CompraDto>> GetComprasForExport(int kioscoId, CancellationToken cancellationToken, DateTime? fechaInicio = null, DateTime? fechaFin = null, int? limite = null);
    }
}