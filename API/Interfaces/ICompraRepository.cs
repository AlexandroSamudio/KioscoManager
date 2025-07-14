using API.DTOs;
using API.Helpers;

namespace API.Interfaces
{
    public interface ICompraRepository
    {
        Task<PagedList<CompraDto>> GetComprasAsync(int kioscoId, int pageNumber, int pageSize, CancellationToken cancellationToken, DateTime? fechaDesde = null, DateTime? fechaHasta = null, string? sortColumn = null, string? sortDirection = null);
        Task<CompraDto?> GetCompraByIdAsync(int kioscoId, int id, CancellationToken cancellationToken);
        Task<IReadOnlyList<CompraDto>> GetComprasRecientesAsync(int kioscoId, int cantidad, CancellationToken cancellationToken);
        Task<decimal> GetTotalComprasDelPeriodoAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fechaDesde = null, DateTime? fechaHasta = null);
        Task<Result<CompraDto>> CreateCompraAsync(CompraCreateDto compraData, int kioscoId, int usuarioId, CancellationToken cancellationToken);
    }
}
