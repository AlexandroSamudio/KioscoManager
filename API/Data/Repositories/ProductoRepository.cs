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
                .Include(p => p.Categoria)
                .ProjectTo<ProductoDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<ProductoDto>> GetProductosAsync()
        {
            return await _context.Productos!
                .Include(p => p.Categoria)
                .ProjectTo<ProductoDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductoDto>> GetProductosByLowestStockAsync()
        {
            return await _context.Productos!
                .Where(p => p.Stock <= 3)
                .Include(p => p.Categoria)
                .ProjectTo<ProductoDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }
    }
}
