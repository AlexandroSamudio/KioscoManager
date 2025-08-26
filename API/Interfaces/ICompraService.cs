using API.DTOs;
using API.Entities;

namespace API.Interfaces
{
    public interface ICompraService
    {
        Task<Result<CompraDto>> CreateCompraAsync(CompraCreateDto compraData, int kioscoId, int usuarioId, CancellationToken cancellationToken);
        Task<Result<CompraDto>> GetCompraByIdAsync(int kioscoId, int compraId, CancellationToken cancellationToken);
        Task<Result<IReadOnlyList<CompraDto>>> GetComprasForExportAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fechaInicio = null, DateTime? fechaFin = null, int? limite = null);
    }
}
