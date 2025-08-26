using API.Constants;
using API.DTOs;
using API.Entities;
using API.Interfaces;

namespace API.Services
{
    public class CompraService(ICompraRepository compraRepository) : ICompraService
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
            var validationResult = ValidateExportParameters(fechaInicio, fechaFin, limite);
            if (!validationResult.IsSuccess)
            {
                return Result<IReadOnlyList<CompraDto>>.Failure(validationResult.ErrorCode ?? ErrorCodes.InvalidOperation, validationResult.Message);
            }

            var compras = await compraRepository.GetComprasForExportAsync(kioscoId, cancellationToken, fechaInicio, fechaFin, validationResult.Data);
            return Result<IReadOnlyList<CompraDto>>.Success(compras);
        }

        private static Result<int> ValidateExportParameters(DateTime? fechaInicio, DateTime? fechaFin, int? limite)
        {
            const int DEFAULT_EXPORT_LIMIT = 5000;
            var limiteAplicar = limite ?? DEFAULT_EXPORT_LIMIT;

            if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio > fechaFin)
            {
                return Result<int>.Failure(ErrorCodes.InvalidOperation, "La fecha de inicio no puede ser posterior a la fecha de fin.");
            }
            if (limite.HasValue && limite <= 0)
            {
                return Result<int>.Failure(ErrorCodes.InvalidOperation, "El límite debe ser mayor que cero.");
            }
            if (limite.HasValue && limite > DEFAULT_EXPORT_LIMIT)
            {
                return Result<int>.Failure(ErrorCodes.InvalidOperation, $"El límite no puede ser mayor a {DEFAULT_EXPORT_LIMIT} registros.");
            }

            return Result<int>.Success(limiteAplicar);
        }
    }
}
