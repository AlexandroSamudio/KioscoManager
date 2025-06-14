using API.DTOs;
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

        public async Task<ProductoDto?> GetProductoByIdAsync(int kioscoId,int id,CancellationToken cancellationToken = default)
        {
            return await _context.Productos!
                .Where(p => p.Id == id && p.KioscoId == kioscoId)
                .AsNoTracking()
                            .ProjectTo<ProductoDto>(_mapper.ConfigurationProvider)
                            .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ProductoDto>> GetProductosAsync(int kioscoId,CancellationToken cancellationToken = default)
        {
            return await _context.Productos!
                .Where(p => p.KioscoId == kioscoId)
                .AsNoTracking()
                .ProjectTo<ProductoDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ProductoDto>> GetProductosByLowestStockAsync(int kioscoId,CancellationToken cancellationToken = default)
        {
            const int LowStockThreshold = 3;
            return await _context.Productos!
                .Where(p => p.Stock <= LowStockThreshold && p.KioscoId == kioscoId)
                .AsNoTracking()
                .ProjectTo<ProductoDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
