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

        public async Task<ProductoDto?> GetProductoByIdAsync(int id)
        {
            return await _context.Productos!
                .Where(p => p.Id == id)
                .AsNoTracking()
                            .ProjectTo<ProductoDto>(_mapper.ConfigurationProvider)
                            .SingleOrDefaultAsync();
        }

        public IAsyncEnumerable<ProductoDto> GetProductosAsync()
        {
            return _context.Productos!
                .AsNoTracking()
                .ProjectTo<ProductoDto>(_mapper.ConfigurationProvider)
                .AsAsyncEnumerable();
        }

        public IAsyncEnumerable<ProductoDto> GetProductosByLowestStockAsync()
        {
            const int LowStockThreshold = 3;
            return _context.Productos!
                .Where(p => p.Stock <= LowStockThreshold)
                .AsNoTracking()
                .ProjectTo<ProductoDto>(_mapper.ConfigurationProvider)
                .AsAsyncEnumerable();
        }
    }
}
