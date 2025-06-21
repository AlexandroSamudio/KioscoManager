using API.DTOs;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public ProductoRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ProductoDto?> GetProductoByIdAsync(int kioscoId,int id,CancellationToken cancellationToken)
        {
            return await _context.Productos!
                .Where(p => p.Id == id && p.KioscoId == kioscoId)
                .AsNoTracking()
                            .ProjectTo<ProductoDto>(_mapper.ConfigurationProvider)
                            .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<PagedList<ProductoDto>> GetProductosAsync(
            int kioscoId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken,
            int? categoriaId = null,
            string? stockStatus = null,
            string? searchTerm = null,
            string? sortColumn = null,
            string? sortDirection = null)
        {
            var query = _context.Productos!
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

            // Ordenamiento dinámico
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

            return await PagedList<ProductoDto>.CreateAsync(query.ProjectTo<ProductoDto>(_mapper.ConfigurationProvider),
                pageNumber, pageSize, cancellationToken);
        }

        public async Task<IReadOnlyList<ProductoDto>> GetProductosByLowestStockAsync(int cantidad,int kioscoId,CancellationToken cancellationToken)
        {
            const int LowStockThreshold = 3;
            return await _context.Productos!
                .Where(p => p.Stock <= LowStockThreshold && p.KioscoId == kioscoId)
                .AsNoTracking()
                .Take(cantidad) 
                .ProjectTo<ProductoDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> DeleteProductoAsync(int kioscoId, int id, CancellationToken cancellationToken)
        {
            var producto = await _context.Productos!
                .FirstOrDefaultAsync(p => p.Id == id && p.KioscoId == kioscoId, cancellationToken);
            if (producto == null) return false;

            _context.Productos!.Remove(producto);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<ProductoDto?> CreateProductoAsync(int kioscoId, ProductoCreateDto dto, CancellationToken cancellationToken)
        {
            var exists = await _context.Productos!
                .AnyAsync(p => p.KioscoId == kioscoId && p.Sku == dto.Sku, cancellationToken);
            if (exists)
            {
                return null;
            }

            var producto = _mapper.Map<Entities.Producto>(dto);
            producto.KioscoId = kioscoId;

            _context.Productos!.Add(producto);
            await _context.SaveChangesAsync(cancellationToken);

            return await _context.Productos!
                .Where(p => p.Id == producto.Id)
                .ProjectTo<ProductoDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<ProductoDto?> UpdateProductoAsync(int kioscoId, int id, ProductoCreateDto dto, CancellationToken cancellationToken)
        {
            var producto = await _context.Productos!
                .FirstOrDefaultAsync(p => p.Id == id && p.KioscoId == kioscoId, cancellationToken);
            if (producto == null) return null;

            var exists = await _context.Productos!
                .AnyAsync(p => p.KioscoId == kioscoId && p.Sku == dto.Sku && p.Id != id, cancellationToken);
            if (exists)
            {
                return null;
            }

            _mapper.Map(dto, producto);

            await _context.SaveChangesAsync(cancellationToken);

            return await _context.Productos!
                .Where(p => p.Id == producto.Id)
                .ProjectTo<ProductoDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<ProductoDto?> GetProductoBySkuAsync(int kioscoId, string sku, CancellationToken cancellationToken)
        {
            return await _context.Productos!
                .Where(p => p.KioscoId == kioscoId && p.Sku == sku)
                .AsNoTracking()
                .ProjectTo<ProductoDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}
