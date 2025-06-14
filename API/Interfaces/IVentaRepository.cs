using API.DTOs;

namespace API.Interfaces
{
    public interface IVentaRepository
    {
        Task<List<VentaDto>> GetVentasDelDiaAsync(int kioscoId, CancellationToken cancellationToken = default);
        Task<List<VentaDto>> GetVentasDelDiaAsync(int kioscoId, DateTime fecha, CancellationToken cancellationToken = default);
        Task<List<VentaDto>> GetVentasRecientesAsync(int kioscoId, int cantidad, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalVentasDelDiaAsync(int kioscoId, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalVentasDelDiaAsync(int kioscoId, DateTime fecha, CancellationToken cancellationToken = default);
        Task<List<ProductoMasVendidoDto>> GetProductosMasVendidosDelDiaAsync(int kioscoId, int cantidad, CancellationToken cancellationToken = default);
        Task<List<ProductoMasVendidoDto>> GetProductosMasVendidosDelDiaAsync(int kioscoId, DateTime fecha, int cantidad, CancellationToken cancellationToken = default);
    }
}
