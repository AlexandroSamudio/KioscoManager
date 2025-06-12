using API.DTOs;

namespace API.Interfaces
{
    public interface IVentaRepository
    {
        IAsyncEnumerable<VentaDto> GetVentasDelDiaAsync(CancellationToken cancellationToken = default);
        IAsyncEnumerable<VentaDto> GetVentasDelDiaAsync(DateTime fecha, CancellationToken cancellationToken = default);
        IAsyncEnumerable<VentaDto> GetVentasRecientesAsync(int cantidad, CancellationToken cancellationToken = default);
    }
}
