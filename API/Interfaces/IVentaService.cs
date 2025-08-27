using API.DTOs;
using API.Entities;

namespace API.Interfaces
{
    public interface IVentaService
    {
        Task<Result<VentaDto>> CreateVentaAsync(VentaCreateDto ventaCreateDto, int kioscoId, int usuarioId, CancellationToken cancellationToken);
        Task<Result<IReadOnlyList<VentaDto>>> GetVentasDelDiaAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fecha = null);
        Task<Result<IReadOnlyList<VentaDto>>> GetVentasRecientesAsync(int kioscoId, int cantidad, CancellationToken cancellationToken);
        Result<IAsyncEnumerable<VentaDto>> GetVentasForExportAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fechaInicio = null, DateTime? fechaFin = null, int? limite = null);
        Task<Result<decimal>> GetMontoTotalVentasDelDiaAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fecha = null);
        Task<Result<IReadOnlyList<ProductoMasVendidoDto>>> GetProductosMasVendidosDelDiaAsync(int kioscoId, int cantidad, CancellationToken cancellationToken, DateTime? fecha = null);
        Task<Result<IReadOnlyList<VentaChartDto>>> GetVentasIndividualesDelDiaAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fecha = null);
    }
}
