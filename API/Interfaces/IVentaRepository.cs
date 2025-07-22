using API.DTOs;
using API.Entities;

namespace API.Interfaces
{
  public interface IVentaRepository
  {
    Task<IReadOnlyList<VentaDto>> GetVentasDelDiaAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fecha = null);
    Task<IReadOnlyList<VentaDto>> GetVentasRecientesAsync(int kioscoId, int cantidad, CancellationToken cancellationToken);
    Task<IReadOnlyList<VentaDto>> GetVentasForExportAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fechaInicio = null, DateTime? fechaFin = null, int? limite = null);
    Task<decimal> GetTotalVentasDelDiaAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fecha = null);
    Task<IReadOnlyList<ProductoMasVendidoDto>> GetProductosMasVendidosDelDiaAsync(int kioscoId, int cantidad, CancellationToken cancellationToken, DateTime? fecha = null);
    Task<IReadOnlyList<VentaChartDto>> GetVentasIndividualesDelDiaAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fecha = null);
    Task<Result<VentaDto>> CreateVentaAsync(VentaCreateDto ventaData, int kioscoId, int usuarioId, CancellationToken cancellationToken = default);
  }
}
