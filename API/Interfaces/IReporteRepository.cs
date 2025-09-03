using API.DTOs;
using API.Helpers;
using API.Entities;

namespace API.Interfaces
{
    public interface IReporteRepository
    {
        Task<Result<ReporteDto>> CalculateKpiReporteAsync(
            int kioscoId,
            DateTime fechaInicio,
            DateTime fechaFin,
            CancellationToken cancellationToken);

        Task<Result<PagedList<ProductoMasVendidoDto>>> GetTopProductsByVentasAsync(
            int kioscoId,
            int pageNumber,
            int pageSize,
            DateTime fechaInicio,
            DateTime fechaFin,
            CancellationToken cancellationToken,
            int limit = 5);

        Task<Result<IReadOnlyList<VentasPorDiaDto>>> GetVentasPorDiaAsync(
            int kioscoId,
            DateTime fechaInicio,
            DateTime fechaFin,
            CancellationToken cancellationToken);

        Task<Result<IReadOnlyList<CategoriasRentabilidadDto>>> GetCategoriasRentabilidadAsync(
            int kioscoId,
            DateTime fechaInicio,
            DateTime fechaFin,
            CancellationToken cancellationToken);
    }
}
