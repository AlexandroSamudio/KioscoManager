using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Constants;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace API.Data.Repositories
{
    public class CompraRepository(DataContext context, IMapper mapper) : ICompraRepository
    {
        public async Task<Result<CompraDto>> GetCompraByIdAsync(int kioscoId, int id, CancellationToken cancellationToken)
        {
            var compra = await context.Compras!
                .Where(c => c.Id == id && c.KioscoId == kioscoId)
                .AsNoTracking()
                .ProjectTo<CompraDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(cancellationToken);
            
            if(compra == null)
            {
                return Result<CompraDto>.Failure(ErrorCodes.EntityNotFound, "Compra no encontrada");
            }

            return Result<CompraDto>.Success(compra);
        }

        public IQueryable<Compra> GetComprasQueryable(int kioscoId)
        {
            return context.Compras!.Where(c => c.KioscoId == kioscoId);
        }

        public IAsyncEnumerable<Compra> GetComprasAsync(int kioscoId, CancellationToken cancellationToken)
        {
            return context.Compras!.Where(c => c.KioscoId == kioscoId)
            .AsNoTracking()
            .AsAsyncEnumerable();
        }

        public async Task<Dictionary<int, Producto>> GetProductosByIdsAsync(int kioscoId, IEnumerable<int> productosIds, CancellationToken cancellationToken)
        {
            return await context.Productos!
                .Where(p => p.KioscoId == kioscoId && productosIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p, cancellationToken);
        }

        public async Task<Result<CompraDto>> CreateCompraWithStockAdjustmentsAsync(CompraCreateDto compraData, int kioscoId, int usuarioId, CancellationToken cancellationToken)
        {
            var productosIds = compraData.Detalles.Select(p => p.ProductoId).Distinct().ToList();
            var productos = await GetProductosByIdsAsync(kioscoId, productosIds, cancellationToken);

            if (productos.Count != productosIds.Count)
            {
                var noEncontrados = productosIds.Except(productos.Keys).ToList();
                return Result<CompraDto>.Failure(ErrorCodes.EntityNotFound, $"Los siguientes id de productos no se encontraron en el kiosco: {string.Join(", ", noEncontrados)}");
            }

            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var compra = mapper.Map<Compra>(compraData);
                compra.KioscoId = kioscoId;
                compra.UsuarioId = usuarioId;
                compra.Fecha = DateTime.UtcNow;
                compra.CostoTotal = Math.Round(compra.Detalles.Sum(d => d.CostoUnitario * d.Cantidad), 2, MidpointRounding.ToEven);

                var entityEntry = context.Compras!.Add(compra);
                var savedCompra = entityEntry.Entity;

                foreach (var detalle in compra.Detalles)
                {
                    var producto = productos[detalle.ProductoId];
                    producto.Stock += detalle.Cantidad;

                    if (producto.PrecioCompra != detalle.CostoUnitario)
                    {
                        producto.PrecioCompra = detalle.CostoUnitario;
                    }
                }
                context.Productos!.UpdateRange(productos.Values);

                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var compraDto = await context.Compras!
                    .AsNoTracking()
                    .Where(c => c.Id == savedCompra.Id && c.KioscoId == kioscoId)
                    .ProjectTo<CompraDto>(mapper.ConfigurationProvider)
                    .SingleAsync(cancellationToken);
                
                return Result<CompraDto>.Success(compraDto);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<CompraDto>.Failure(ErrorCodes.InvalidOperation, "Error al guardar los datos en la base de datos");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<CompraDto>.Failure(ErrorCodes.InvalidOperation, "Error al procesar la compra");
            }
        }


        public Result<IAsyncEnumerable<CompraDto>> GetComprasForExport(
            int kioscoId,
            CancellationToken cancellationToken,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            int? limite = null)
        {
            var stream = GetComprasForExportInternalAsync(kioscoId, cancellationToken, fechaInicio, fechaFin, limite);
            return Result<IAsyncEnumerable<CompraDto>>.Success(stream);
        }

        private async IAsyncEnumerable<CompraDto> GetComprasForExportInternalAsync(int kioscoId, [EnumeratorCancellation] CancellationToken cancellationToken, DateTime? fechaInicio = null, DateTime? fechaFin = null, int? limite = null)
        {
            var query = GetComprasQueryable(kioscoId);

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

            query = query.OrderByDescending(v => v.Fecha);
            if (limite.HasValue)
            {
                query = query.Take(limite.Value);
            }

            var compras = query
                .AsNoTracking()
                .ProjectTo<CompraDto>(mapper.ConfigurationProvider)
                .AsAsyncEnumerable();

            await foreach (var compra in compras.WithCancellation(cancellationToken))
            {
                yield return compra;
            }
        }
    }
}
