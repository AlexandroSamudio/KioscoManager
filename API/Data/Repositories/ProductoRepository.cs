using API.Constants;
using API.DTOs;
using API.Helpers;
using API.Interfaces;
using API.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace API.Data.Repositories
{
    public class ProductoRepository(DataContext context, IMapper mapper) : IProductoRepository
    {
        private const int LowStockThreshold = 3;
        public async Task<Result<ProductoDto>> GetProductoByIdAsync(int kioscoId, int id, CancellationToken cancellationToken)
        {
            var producto = await context.Productos!
                .Where(p => p.Id == id && p.KioscoId == kioscoId)
                .AsNoTracking()
                .ProjectTo<ProductoDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (producto == null)
            {
                return Result<ProductoDto>.Failure(ErrorCodes.EntityNotFound, "Producto no encontrado");
            }

            return Result<ProductoDto>.Success(producto);
        }

        public async Task<Result<PagedList<ProductoDto>>> GetProductosPaginatedAsync(
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
                        query = query.Where(p => p.Stock > 0 && p.Stock <= LowStockThreshold);
                        break;
                    case "out":
                        query = query.Where(p => p.Stock == 0);
                        break;
                    case "in":
                        query = query.Where(p => p.Stock > LowStockThreshold);
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

            var pagedList = await PagedList<ProductoDto>.CreateAsync(query.ProjectTo<ProductoDto>(mapper.ConfigurationProvider),
                pageNumber, pageSize, cancellationToken);

            return Result<PagedList<ProductoDto>>.Success(pagedList);
        }

        public async Task<Result<IReadOnlyList<ProductoDto>>> GetProductosByLowestStockAsync(int cantidad, int kioscoId, CancellationToken cancellationToken)
        {
            var productos = await context.Productos!
                .Where(p => p.Stock <= LowStockThreshold && p.KioscoId == kioscoId)
                .OrderBy(p => p.Stock)
                .AsNoTracking()
                .Take(cantidad)
                .ProjectTo<ProductoDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyList<ProductoDto>>.Success(productos);
        }

        public async Task<Result<ProductoDto>> CreateProductoAsync(int kioscoId, ProductoCreateDto createDto, CancellationToken cancellationToken)
        {
            var categoriaExists = await context.Categorias!
                .AnyAsync(c => c.Id == createDto.CategoriaId, cancellationToken);
            
            if (!categoriaExists)
            {
                return Result<ProductoDto>.Failure(ErrorCodes.EntityNotFound, $"La categoría con ID {createDto.CategoriaId} no existe.");
            }

            var exists = await context.Productos!
                .AnyAsync(p => p.KioscoId == kioscoId && p.Sku == createDto.Sku, cancellationToken);

            if (exists)
            {
                return Result<ProductoDto>.Failure(ErrorCodes.FieldExists, "Ya existe un producto con el mismo SKU en este kiosco.");
            }

            var producto = mapper.Map<Producto>(createDto);
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

            if (dto.CategoriaId.HasValue)
            {
                var categoriaExists = await context.Categorias!
                    .AnyAsync(c => c.Id == dto.CategoriaId.Value, cancellationToken);
                
                if (!categoriaExists)
                {
                    return Result.Failure(ErrorCodes.EntityNotFound, $"La categoría con ID {dto.CategoriaId.Value} no existe.");
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Sku))
            {
                var exists = await context.Productos!
                    .AnyAsync(p => p.KioscoId == kioscoId && p.Sku == dto.Sku && p.Id != id, cancellationToken);

                if (exists)
                {
                    return Result.Failure(ErrorCodes.FieldExists, "Ya existe un producto con el mismo SKU en este kiosco.");
                }
            }

            mapper.Map(dto, producto);

            await context.SaveChangesAsync(cancellationToken);

            return Result.Success();
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

        public async Task<Result<ProductoDto>> GetProductoBySkuAsync(int kioscoId, string sku, CancellationToken cancellationToken)
        {
            var producto = await context.Productos!
                .Where(p => p.KioscoId == kioscoId && p.Sku == sku)
                .AsNoTracking()
                .ProjectTo<ProductoDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (producto == null)
            {
                return Result<ProductoDto>.Failure(ErrorCodes.EntityNotFound, $"No se encontró un producto con SKU '{sku}' en este kiosco");
            }

            return Result<ProductoDto>.Success(producto);
        }

        public async Task<Result<decimal>> GetCapitalInvertidoTotalAsync(int kioscoId, CancellationToken cancellationToken)
        {
            var totalCapital = await context.Productos!
                .Where(p => p.KioscoId == kioscoId)
                .SumAsync(p => (decimal?)(p.PrecioCompra * p.Stock) ?? 0, cancellationToken);

            return Result<decimal>.Success(totalCapital);
        }

        public async Task<Result<int>> GetTotalProductosUnicosAsync(int kioscoId, CancellationToken cancellationToken)
        {
            var total = await context.Productos!
                .Where(p => p.KioscoId == kioscoId)
                .CountAsync(cancellationToken);

            return Result<int>.Success(total);
        }

        public Result<IAsyncEnumerable<ProductoDto>> GetProductosForExport(
            int kioscoId,
            CancellationToken cancellationToken,
            int? limite = null)
        {
            var stream = GetProductosForExportInternalAsync(kioscoId, cancellationToken, limite);
            return Result<IAsyncEnumerable<ProductoDto>>.Success(stream);
        }

        private async IAsyncEnumerable<ProductoDto> GetProductosForExportInternalAsync(int kioscoId, [EnumeratorCancellation] CancellationToken cancellationToken, int? limite = null)
        {
            var query = context.Productos!
                .Where(v => v.KioscoId == kioscoId)
                .AsNoTracking();

            query = query.OrderByDescending(v => v.Id);
            if (limite.HasValue)
            {
                query = query.Take(limite.Value);
            }

            var productos = query
                .ProjectTo<ProductoDto>(mapper.ConfigurationProvider)
                .AsAsyncEnumerable();

            await foreach (var producto in productos.WithCancellation(cancellationToken))
            {
                yield return producto;
            }
        }
    }
}
