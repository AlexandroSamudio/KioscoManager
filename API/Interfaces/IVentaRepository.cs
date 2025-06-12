using API.DTOs;

namespace API.Interfaces
{
    public interface IVentaRepository
    {
        Task<List<VentaDto>> GetVentasDelDiaAsync(CancellationToken cancellationToken = default);
        Task<List<VentaDto>> GetVentasDelDiaAsync(DateTime fecha, CancellationToken cancellationToken = default);
        Task<List<VentaDto>> GetVentasRecientesAsync(int cantidad, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalVentasDelDiaAsync(CancellationToken cancellationToken = default);
        Task<decimal> GetTotalVentasDelDiaAsync(DateTime fecha, CancellationToken cancellationToken = default);
    }
}
