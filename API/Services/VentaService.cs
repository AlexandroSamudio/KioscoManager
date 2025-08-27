using API.Constants;
using API.DTOs;
using API.Entities;
using API.Interfaces;

namespace API.Services
{
    public class VentaService(IVentaRepository ventaRepository) : IVentaService
    {
        public async Task<Result<VentaDto>> CreateVentaAsync(VentaCreateDto ventaCreateDto, int kioscoId, int usuarioId, CancellationToken cancellationToken)
        {
            return await ventaRepository.CreateVentaAsync(ventaCreateDto, kioscoId, usuarioId, cancellationToken);
        }

        public async Task<Result<IReadOnlyList<VentaDto>>> GetVentasDelDiaAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fecha = null)
        {
            var ventas = await ventaRepository.GetVentasDelDiaAsync(kioscoId, cancellationToken, fecha);
            
            if (ventas is null || !ventas.Any())
            {
                return Result<IReadOnlyList<VentaDto>>.Failure(ErrorCodes.EntityNotFound, "No se encontraron ventas para la fecha especificada.");
            }

            return Result<IReadOnlyList<VentaDto>>.Success(ventas);
        }

        public async Task<Result<IReadOnlyList<VentaDto>>> GetVentasRecientesAsync(int kioscoId, int cantidad, CancellationToken cancellationToken)
        {
            if(cantidad <= 0 || cantidad > 10)
            {
                return Result<IReadOnlyList<VentaDto>>.Failure(ErrorCodes.ValidationError, "La cantidad debe estar entre 1 y 10.");
            }

            var ventas = await ventaRepository.GetVentasRecientesAsync(kioscoId, cantidad, cancellationToken);

            if (ventas is null || !ventas.Any())
            {
                return Result<IReadOnlyList<VentaDto>>.Failure(ErrorCodes.EntityNotFound, "No se encontraron ventas recientes.");
            }

            return Result<IReadOnlyList<VentaDto>>.Success(ventas);
        }

        public Result<IAsyncEnumerable<VentaDto>> GetVentasForExportAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fechaInicio = null, DateTime? fechaFin = null, int? limite = null)
        {
            const int MAX_EXPORT_LIMIT = 5000;

            if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio > fechaFin)
            {
                return Result<IAsyncEnumerable<VentaDto>>.Failure(ErrorCodes.ValidationError, "La fecha de inicio no puede ser posterior a la fecha de fin.");
            }

            if (limite.HasValue && limite <= 0)
            {
                return Result<IAsyncEnumerable<VentaDto>>.Failure(ErrorCodes.ValidationError, "El límite debe ser mayor que cero.");
            }

            if (limite.HasValue && limite > MAX_EXPORT_LIMIT)
            {
                return Result<IAsyncEnumerable<VentaDto>>.Failure(ErrorCodes.ValidationError, $"El límite no puede ser mayor a {MAX_EXPORT_LIMIT:N0} registros.");
            }

            var ventas = ventaRepository.GetVentasForExportAsync(kioscoId, cancellationToken, fechaInicio, fechaFin, limite);
            return Result<IAsyncEnumerable<VentaDto>>.Success(ventas);
        }

        public async Task<Result<decimal>> GetMontoTotalVentasDelDiaAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fecha = null)
        {
            var montoVentas = await ventaRepository.GetMontoTotalVentasDelDiaAsync(kioscoId, cancellationToken, fecha);

            if (montoVentas == 0)
            {
                return Result<decimal>.Failure(ErrorCodes.EntityNotFound, "No se encontraron ventas registradas para la fecha especificada.");
            }

            return Result<decimal>.Success(montoVentas);
        }

        public async Task<Result<IReadOnlyList<ProductoMasVendidoDto>>> GetProductosMasVendidosDelDiaAsync(int kioscoId, int cantidad, CancellationToken cancellationToken, DateTime? fecha = null)
        {
            if (cantidad <= 0)
            {
                return Result<IReadOnlyList<ProductoMasVendidoDto>>.Failure(ErrorCodes.ValidationError, "La cantidad debe ser mayor a 0.");
            }

            var productos = await ventaRepository.GetProductosMasVendidosDelDiaAsync(kioscoId, cantidad, cancellationToken, fecha);
            if (productos is null || !productos.Any())
            {
                return Result<IReadOnlyList<ProductoMasVendidoDto>>.Failure(ErrorCodes.EntityNotFound, "No se encontraron productos vendidos para la fecha especificada.");
            }

            return Result<IReadOnlyList<ProductoMasVendidoDto>>.Success(productos);
        }

        public async Task<Result<IReadOnlyList<VentaChartDto>>> GetVentasIndividualesDelDiaAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fecha = null)
        {
            var ventas = await ventaRepository.GetVentasIndividualesDelDiaAsync(kioscoId, cancellationToken, fecha);
            if (ventas is null || !ventas.Any())
            {
                return Result<IReadOnlyList<VentaChartDto>>.Failure(ErrorCodes.EntityNotFound, "No se encontraron ventas para la fecha especificada.");
            }

            return Result<IReadOnlyList<VentaChartDto>>.Success(ventas);
        }
    }
}
