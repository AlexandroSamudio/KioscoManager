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

        public async Task<PagedList<ProductoDto>> GetProductosAsync(int kioscoId, int pageNumber, int pageSize, CancellationToken cancellationToken, int? categoriaId = null, string? stockStatus = null)
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

            query = query.OrderBy(p => p.Stock);

            return await PagedList<ProductoDto>.CreateAsync(query.ProjectTo<ProductoDto>(_mapper.ConfigurationProvider),
                pageNumber, pageSize, cancellationToken);
        }

        public async Task<IReadOnlyList<ProductoDto>> GetProductosByLowestStockAsync(int kioscoId,int cantidad,CancellationToken cancellationToken)
        {
            const int LowStockThreshold = 3;
            return await _context.Productos!
                .Where(p => p.Stock <= LowStockThreshold && p.KioscoId == kioscoId)
                .AsNoTracking()
                .Take(cantidad) 
                .ProjectTo<ProductoDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
