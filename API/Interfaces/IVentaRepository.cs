using API.DTOs;

namespace API.Interfaces
{
    public interface IVentaRepository
    {
        IAsyncEnumerable<VentaDto> GetVentasDelDiaAsync();
        IAsyncEnumerable<VentaDto> GetVentasDelDiaAsync(DateTime fecha);
        IAsyncEnumerable<VentaDto> GetVentasRecientesAsync(int cantidad);
    }
}
