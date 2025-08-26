using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Constants;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public class CompraRepository(DataContext context, IMapper mapper) : ICompraRepository
    {
        public async Task<CompraDto?> GetCompraByIdAsync(int kioscoId, int id, CancellationToken cancellationToken)
        {
            return await context.Compras!
                .Where(c => c.Id == id && c.KioscoId == kioscoId)
                .AsNoTracking()
                .ProjectTo<CompraDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(cancellationToken);
        }

        public IQueryable<Compra> GetComprasQueryable(int kioscoId)
        {
            return context.Compras!.Where(c => c.KioscoId == kioscoId);
        }

        public IAsyncEnumerable<Compra> GetComprasAsync(int kioscoId, CancellationToken cancellationToken)
        {
            return context.Compras!.Where(c => c.KioscoId == kioscoId).AsAsyncEnumerable();
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
                    .Where(c => c.Id == savedCompra.Id)
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
    }
}
