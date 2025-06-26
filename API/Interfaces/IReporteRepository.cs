using API.DTOs;
using API.Helpers;

namespace API.Interfaces
{
    public interface IReporteRepository
    {
        Task<ReporteDto> CalculateKpiReporteAsync(
            int kioscoId,
            DateTime fechaInicio,
            DateTime fechaFin,
            CancellationToken cancellationToken = default);

        Task<PagedList<ProductoMasVendidoDto>> GetTopProductsByVentasAsync(
            int kioscoId,
            int pageNumber,
            int pageSize,
            DateTime fechaInicio,
            DateTime fechaFin,
            int limit = 5,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<VentasPorDiaDto>> GetVentasPorDiaAsync(
            int kioscoId,
            DateTime fechaInicio,
            DateTime fechaFin,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<CategoriasRentabilidadDto>> GetCategoriasRentabilidadAsync(
            int kioscoId,
            DateTime fechaInicio,
            DateTime fechaFin,
            CancellationToken cancellationToken = default);
    }
}
