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
        private readonly DataContext _context = context;
        private readonly IMapper _mapper = mapper;

        public async Task<Result<CompraDto>> CreateCompraAsync(CompraCreateDto compraData, int kioscoId, int usuarioId, CancellationToken cancellationToken = default)
        {
            var productosIds = compraData.Detalles.Select(p => p.ProductoId).Distinct().ToList();

            var productos = await _context.Productos!
                .Where(p => p.KioscoId == kioscoId && productosIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p, cancellationToken);

            if (productos.Count != productosIds.Count)
            {
                var noEncontrados = productosIds.Except(productos.Keys).ToList();
                return Result<CompraDto>.Failure(CompraErrorCodes.ProductoNoEncontrado, $"Los siguientes productos no se encontraron en el kiosco: {string.Join(", ", noEncontrados)}");
            }

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
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

                var compra = _mapper.Map<Compra>(compraData);
                compra.KioscoId = kioscoId;
                compra.UsuarioId = usuarioId;
                compra.Fecha = DateTime.UtcNow;
                compra.CostoTotal = compra.Detalles.Sum(d => d.CostoUnitario * d.Cantidad);

                _context.Compras!.Add(compra);

                foreach (var detalle in compra.Detalles)
                {
                    var producto = productos[detalle.ProductoId];

                    producto.Stock += detalle.Cantidad;

                    if (producto.PrecioCompra != detalle.CostoUnitario)
                    {
                        producto.PrecioCompra = detalle.CostoUnitario;
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                var compraDto = _mapper.Map<CompraDto>(compra);

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
            return await _context.Compras!
                .Where(c => c.Id == id && c.KioscoId == kioscoId)
                .AsNoTracking()
                .ProjectTo<CompraDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<PagedList<CompraDto>> GetComprasAsync(
            int kioscoId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            string? sortColumn = null,
            string? sortDirection = null)
        {
            var query = _context.Compras!
                .Where(c => c.KioscoId == kioscoId)
                .AsNoTracking();

            if (fechaDesde.HasValue)
            {
                var fechaDesdeUtc = ToUtcDateStart(fechaDesde.Value);
                query = query.Where(c => c.Fecha >= fechaDesdeUtc);
            }

            if (fechaHasta.HasValue)
            {
                var fechaHastaUtc = ToUtcDateEnd(fechaHasta.Value);
                query = query.Where(c => c.Fecha < fechaHastaUtc);
            }

            query = (sortColumn, sortDirection?.ToLower()) switch
            {
                ("fecha", "desc") => query.OrderByDescending(c => c.Fecha),
                ("fecha", _) => query.OrderBy(c => c.Fecha),
                ("proveedor", "desc") => query.OrderByDescending(c => c.Proveedor),
                ("proveedor", _) => query.OrderBy(c => c.Proveedor),
                ("costoTotal", "desc") => query.OrderByDescending(c => c.CostoTotal),
                ("costoTotal", _) => query.OrderBy(c => c.CostoTotal),
                _ => query.OrderByDescending(c => c.Fecha)
            };

            return await PagedList<CompraDto>.CreateAsync(
                query.ProjectTo<CompraDto>(_mapper.ConfigurationProvider),
                pageNumber,
                pageSize,
                cancellationToken);
        }

        public async Task<IReadOnlyList<CompraDto>> GetComprasRecientesAsync(int kioscoId, int cantidad, CancellationToken cancellationToken)
        {
            return await _context.Compras!
                .Where(c => c.KioscoId == kioscoId)
                .OrderByDescending(c => c.Fecha)
                .Take(cantidad)
                .AsNoTracking()
                .ProjectTo<CompraDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<decimal> GetTotalComprasDelPeriodoAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var query = _context.Compras!
                .Where(c => c.KioscoId == kioscoId)
                .AsNoTracking();

            if (fechaDesde.HasValue)
            {
                var fechaDesdeUtc = ToUtcDateStart(fechaDesde.Value);
                query = query.Where(c => c.Fecha >= fechaDesdeUtc);
            }

            if (fechaHasta.HasValue)
            {
                var fechaHastaUtc = ToUtcDateEnd(fechaHasta.Value);
                query = query.Where(c => c.Fecha < fechaHastaUtc);
            }

            return await query.SumAsync(c => c.CostoTotal, cancellationToken);
        }

        private static DateTime ToUtcDateStart(DateTime date)
        {
            return date.Kind == DateTimeKind.Utc ? date.Date : date.ToUniversalTime().Date;
        }

        private static DateTime ToUtcDateEnd(DateTime date)
        {
            return date.Kind == DateTimeKind.Utc ? date.Date.AddDays(1) : date.ToUniversalTime().Date.AddDays(1);
        }
    }
}
