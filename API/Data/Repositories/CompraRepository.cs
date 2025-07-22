using API.Constants;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public class CompraRepository(DataContext context, IMapper mapper) : ICompraRepository
    {
        public async Task<Result<CompraDto>> CreateCompraAsync(CompraCreateDto compraData, int kioscoId, int usuarioId, CancellationToken cancellationToken = default)
        {
            var productosIds = compraData.Detalles.Select(p => p.ProductoId).Distinct().ToList();

            var productos = await context.Productos!
                .Where(p => p.KioscoId == kioscoId && productosIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p, cancellationToken);

            if (productos.Count != productosIds.Count)
            {
                var noEncontrados = productosIds.Except(productos.Keys).ToList();
                return Result<CompraDto>.Failure(CompraErrorCodes.ProductoNoEncontrado, $"Los siguientes productos no se encontraron en el kiosco: {string.Join(", ", noEncontrados)}");
            }

            using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (var productoCompra in compraData.Detalles)
                {
                    if (productoCompra.Cantidad <= 0)
                    {
                        return Result<CompraDto>.Failure(CompraErrorCodes.CantidadInvalida, $"La cantidad para el producto con ID {productoCompra.ProductoId} debe ser mayor que cero");
                    }

                    if (productoCompra.CostoUnitario <= 0)
                    {
                        return Result<CompraDto>.Failure(CompraErrorCodes.CostoUnitarioInvalido, $"El costo unitario para el producto con ID {productoCompra.ProductoId} debe ser mayor que cero");
                    }
                }

                var compra = mapper.Map<Compra>(compraData);
                compra.KioscoId = kioscoId;
                compra.UsuarioId = usuarioId;
                compra.Fecha = DateTime.UtcNow;
                compra.CostoTotal = compra.Detalles.Sum(d => d.CostoUnitario * d.Cantidad);

                context.Compras!.Add(compra);

                foreach (var detalle in compra.Detalles)
                {
                    var producto = productos[detalle.ProductoId];

                    producto.Stock += detalle.Cantidad;

                    if (producto.PrecioCompra != detalle.CostoUnitario)
                    {
                        producto.PrecioCompra = detalle.CostoUnitario;
                    }
                }

                await context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                var compraDto = mapper.Map<CompraDto>(compra);

                return Result<CompraDto>.Success(compraDto);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<CompraDto>.Failure("Error al guardar los datos en la base de datos");
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<CompraDto>.Failure("Error al procesar la compra");
            }
        }

        public async Task<CompraDto?> GetCompraByIdAsync(int kioscoId, int id, CancellationToken cancellationToken)
        {
            return await context.Compras!
                .Where(c => c.Id == id && c.KioscoId == kioscoId)
                .AsNoTracking()
                .ProjectTo<CompraDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<CompraDto>> GetComprasForExportAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fechaInicio = null, DateTime? fechaFin = null, int? limite = null)
        {
            const int DEFAULT_EXPORT_LIMIT = 5000;
            var limiteAplicar = limite ?? DEFAULT_EXPORT_LIMIT;

            var query = context.Compras!
                .Where(c => c.KioscoId == kioscoId);

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

            return await query
                .OrderByDescending(v => v.Fecha)
                .Take(limiteAplicar)
                .AsNoTracking()
                .ProjectTo<CompraDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
