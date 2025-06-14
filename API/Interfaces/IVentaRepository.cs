using API.DTOs;

namespace API.Interfaces
{
    public interface IVentaRepository
    {
        Task<IReadOnlyList<VentaDto>> GetVentasDelDiaAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fecha = null);
        Task<IReadOnlyList<VentaDto>> GetVentasRecientesAsync(int kioscoId, int cantidad, CancellationToken cancellationToken);
        Task<decimal> GetTotalVentasDelDiaAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fecha = null);
        Task<IReadOnlyList<ProductoMasVendidoDto>> GetProductosMasVendidosDelDiaAsync(int kioscoId, int cantidad, CancellationToken cancellationToken, DateTime? fecha = null);
    }
}
