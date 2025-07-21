using API.DTOs;
using API.Helpers;
using API.Entities;

namespace API.Interfaces
{
    public interface ICompraRepository
    {
        Task<IReadOnlyList<CompraDto>> GetComprasForExportAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fechaInicio = null, DateTime? fechaFin = null, int? limite = null);
        Task<CompraDto?> GetCompraByIdAsync(int kioscoId, int id, CancellationToken cancellationToken);
        Task<Result<CompraDto>> CreateCompraAsync(CompraCreateDto compraData, int kioscoId, int usuarioId, CancellationToken cancellationToken);
    }
}
