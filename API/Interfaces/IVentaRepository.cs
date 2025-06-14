using API.DTOs;

namespace API.Interfaces
{
    public interface IVentaRepository
    {
        Task<IReadOnlyList<VentaDto>> GetVentasDelDiaAsync(int kioscoId, DateTime? fecha = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<VentaDto>> GetVentasRecientesAsync(int kioscoId, int cantidad, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalVentasDelDiaAsync(int kioscoId, DateTime? fecha = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ProductoMasVendidoDto>> GetProductosMasVendidosDelDiaAsync(int kioscoId, int cantidad, DateTime? fecha = null, CancellationToken cancellationToken = default);
    }
}
