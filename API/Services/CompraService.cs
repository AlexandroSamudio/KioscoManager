using API.Constants;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;

namespace API.Services
{
    public class CompraService(ICompraRepository compraRepository, IMapper mapper) : ICompraService
    {
        public async Task<Result<CompraDto>> CreateCompraAsync(CompraCreateDto compraData, int kioscoId, int usuarioId, CancellationToken cancellationToken)
        {
            return await compraRepository.CreateCompraWithStockAdjustmentsAsync(compraData, kioscoId, usuarioId, cancellationToken);
        }

        public async Task<Result<CompraDto>> GetCompraByIdAsync(int kioscoId, int id, CancellationToken cancellationToken)
        {
            var compra = await compraRepository.GetCompraByIdAsync(kioscoId, id, cancellationToken);

            if (compra == null)
            {
                return Result<CompraDto>.Failure(ErrorCodes.EntityNotFound, "Compra no encontrada");
            }

            return Result<CompraDto>.Success(compra);
        }

        public async Task<Result<IReadOnlyList<CompraDto>>> GetComprasForExportAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fechaInicio = null, DateTime? fechaFin = null, int? limite = null)
            {
                const int DEFAULT_EXPORT_LIMIT = 5000;
                var limiteAplicar = limite ?? DEFAULT_EXPORT_LIMIT;

                if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio > fechaFin)
                {
                    return Result<IReadOnlyList<CompraDto>>.Failure(ErrorCodes.InvalidOperation, "La fecha de inicio no puede ser posterior a la fecha de fin.");
                }
                if (limite.HasValue && limite <= 0)
                {
                    return Result<IReadOnlyList<CompraDto>>.Failure(ErrorCodes.InvalidOperation, "El límite debe ser mayor que cero.");
                }
                if (limite.HasValue && limite > DEFAULT_EXPORT_LIMIT)
                {
                    return Result<IReadOnlyList<CompraDto>>.Failure(ErrorCodes.InvalidOperation, $"El límite no puede ser mayor a {DEFAULT_EXPORT_LIMIT} registros.");
                }

                var query = compraRepository.GetComprasQueryable(kioscoId);

                if (fechaInicio.HasValue)
                {
                    var fechaInicioUtc = fechaInicio.Value.Kind == DateTimeKind.Utc
                        ? fechaInicio.Value
                        : fechaInicio.Value.ToUniversalTime();
                    query = query.Where(v => v.Fecha >= fechaInicioUtc);
                }

                if (fechaFin.HasValue)
                {
                    var fechaFinUtc = fechaFin.Value.Kind == DateTimeKind.Utc
                        ? fechaFin.Value
                        : fechaFin.Value.ToUniversalTime();
                    query = query.Where(v => v.Fecha <= fechaFinUtc);
                }

                var compras = await query
                    .OrderByDescending(v => v.Fecha)
                    .Take(limiteAplicar)
                    .AsNoTracking()
                    .ProjectTo<CompraDto>(mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
                return Result<IReadOnlyList<CompraDto>>.Success(compras);
            }
        }
}
