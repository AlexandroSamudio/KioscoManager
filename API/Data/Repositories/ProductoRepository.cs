using API.Constants;
using API.DTOs;
using API.Helpers;
using API.Interfaces;
using API.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public class ProductoRepository(DataContext context, IMapper mapper) : IProductoRepository
    {
        public async Task<ProductoDto?> GetProductoByIdAsync(int kioscoId, int id, CancellationToken cancellationToken)
        {
            return await context.Productos!
                .Where(p => p.Id == id && p.KioscoId == kioscoId)
                .AsNoTracking()
                            .ProjectTo<ProductoDto>(mapper.ConfigurationProvider)
                            .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<PagedList<ProductoDto>> GetProductosPaginatedAsync(
            CancellationToken cancellationToken,
            int kioscoId,
            int pageNumber,
            int pageSize,
            int? categoriaId = null,
            string? stockStatus = null,
            string? searchTerm = null,
            string? sortColumn = null,
            string? sortDirection = null)
        {
            var query = context.Productos!
                .Where(p => p.KioscoId == kioscoId)
                .AsNoTracking();

            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.CategoriaId == categoriaId.Value);
            }

            if (!string.IsNullOrEmpty(stockStatus))
            {
                switch (stockStatus.ToLower())
                {
                    case "low":
                        query = query.Where(p => p.Stock > 0 && p.Stock <= 3);
                        break;
                    case "out":
                        query = query.Where(p => p.Stock == 0);
                        break;
                    case "in":
                        query = query.Where(p => p.Stock > 3);
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.Trim().ToLower();
                query = query.Where(p =>
                    p.Nombre.ToLower().Contains(search) ||
                    p.Sku.ToLower().Contains(search)
                );
            }

            query = (sortColumn, sortDirection?.ToLower()) switch
            {
                ("sku", "desc") => query.OrderByDescending(p => p.Sku),
                ("sku", _) => query.OrderBy(p => p.Sku),
                ("nombre", "desc") => query.OrderByDescending(p => p.Nombre),
                ("nombre", _) => query.OrderBy(p => p.Nombre),
                ("precioCompra", "desc") => query.OrderByDescending(p => p.PrecioCompra),
                ("precioCompra", _) => query.OrderBy(p => p.PrecioCompra),
                ("precioVenta", "desc") => query.OrderByDescending(p => p.PrecioVenta),
                ("precioVenta", _) => query.OrderBy(p => p.PrecioVenta),
                ("stock", "desc") => query.OrderByDescending(p => p.Stock),
                ("stock", _) => query.OrderBy(p => p.Stock),
                _ => query.OrderBy(p => p.Id)
            };

            return await PagedList<ProductoDto>.CreateAsync(query.ProjectTo<ProductoDto>(mapper.ConfigurationProvider),
                pageNumber, pageSize, cancellationToken);
        }

        public async Task<IReadOnlyList<ProductoDto>> GetProductosByLowestStockAsync(int cantidad, int kioscoId, CancellationToken cancellationToken)
        {
            const int LowStockThreshold = 3;
            return await context.Productos!
                .Where(p => p.Stock <= LowStockThreshold && p.KioscoId == kioscoId)
                .OrderBy(p => p.Stock)
                .AsNoTracking()
                .Take(cantidad)
                .ProjectTo<ProductoDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<Result> DeleteProductoAsync(int kioscoId, int id, CancellationToken cancellationToken)
        {
            var producto = await context.Productos!
                .FirstOrDefaultAsync(p => p.Id == id && p.KioscoId == kioscoId, cancellationToken);
            if (producto == null) return Result.Failure(ErrorCodes.EntityNotFound, "Producto no encontrado");

            context.Productos!.Remove(producto);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result<ProductoDto>> CreateProductoAsync(int kioscoId, ProductoCreateDto createDto, CancellationToken cancellationToken)
        {
            var exists = await context.Productos!
                .AnyAsync(p => p.KioscoId == kioscoId && p.Sku == createDto.Sku, cancellationToken);

            if (exists)
            {
                return Result<ProductoDto>.Failure(ErrorCodes.FieldExists, "El SKU ya existe para este kiosco.");
            }

            var producto = mapper.Map<Entities.Producto>(createDto);
            producto.KioscoId = kioscoId;

            context.Productos!.Add(producto);
            await context.SaveChangesAsync(cancellationToken);

            var productoDto = mapper.Map<ProductoDto>(producto);

            return Result<ProductoDto>.Success(productoDto);
        }

        public async Task<Result> UpdateProductoAsync(int kioscoId, int id, ProductoUpdateDto dto, CancellationToken cancellationToken)
        {
            var producto = await context.Productos!
                .FirstOrDefaultAsync(p => p.Id == id && p.KioscoId == kioscoId, cancellationToken);
            if (producto == null) return Result.Failure(ErrorCodes.EntityNotFound, "Producto no encontrado");

            var exists = await context.Productos!
                .AnyAsync(p => p.KioscoId == kioscoId && p.Sku == dto.Sku && p.Id != id, cancellationToken);
            if (exists)
            {
                return Result.Failure(ErrorCodes.FieldExists, "Ya existe un producto con el mismo SKU en este kiosco.");
            }

            mapper.Map(dto, producto);

            await context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<ProductoDto?> GetProductoBySkuAsync(int kioscoId, string sku, CancellationToken cancellationToken)
        {
            return await context.Productos!
                .Where(p => p.KioscoId == kioscoId && p.Sku == sku)
                .AsNoTracking()
                .ProjectTo<ProductoDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<decimal> GetCapitalInvertidoTotalAsync(int kioscoId, CancellationToken cancellationToken)
        {
            var totalCapital = await context.Productos!
                .Where(p => p.KioscoId == kioscoId)
                .SumAsync(p => (decimal?)(p.PrecioCompra * p.Stock) ?? 0, cancellationToken);

            return totalCapital;
        }

        public async Task<int> GetTotalProductosUnicosAsync(int kioscoId, CancellationToken cancellationToken)
        {
            return await context.Productos!
                .Where(p => p.KioscoId == kioscoId)
                .CountAsync(cancellationToken);
        }
        
        public async Task<IReadOnlyList<ProductoDto>> GetProductosForExportAsync(int kioscoId, CancellationToken cancellationToken, int? limite = null)
        {
            const int DEFAULT_EXPORT_LIMIT = 5000;
            var limiteAplicar = limite ?? DEFAULT_EXPORT_LIMIT;

            var query = context.Productos!
                .Where(v => v.KioscoId == kioscoId)
                .AsQueryable();

            return await query
                .Take(limiteAplicar)
                .AsNoTracking()
                .ProjectTo<ProductoDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
