using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public CategoriaRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedList<CategoriaDto>> GetCategoriasAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            var query = _context.Categorias!
                .AsNoTracking()
                .OrderBy(c => c.Nombre)
                .ProjectTo<CategoriaDto>(_mapper.ConfigurationProvider);

            return await PagedList<CategoriaDto>.CreateAsync(query, pageNumber, pageSize, cancellationToken);
        }

        public async Task<CategoriaDto?> GetCategoriaByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _context.Categorias!
                .AsNoTracking()
                .Where(c => c.Id == id)
                .ProjectTo<CategoriaDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<CategoriaDto> CreateCategoriaAsync(CategoriaCreateDto createDto, CancellationToken cancellationToken)
        {   
            if (await CategoriaExistsAsync(createDto.Nombre, cancellationToken: cancellationToken))
            {
                throw new InvalidOperationException($"Ya existe una categoría con el nombre '{createDto.Nombre}'");
            }

            var categoria = _mapper.Map<Categoria>(createDto);
            
            _context.Categorias!.Add(categoria);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CategoriaDto>(categoria);
        }

        public async Task<CategoriaDto?> UpdateCategoriaAsync(int id, CategoriaUpdateDto updateDto, CancellationToken cancellationToken)
        {
            var categoria = await _context.Categorias!
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (categoria == null)
                return null;
            
            if (string.IsNullOrWhiteSpace(updateDto.Nombre))
                {
                    throw new InvalidOperationException("El nombre de la categoría no puede estar vacío");
                }

            if (await CategoriaExistsAsync(updateDto.Nombre, cancellationToken, id))
                {
                    throw new InvalidOperationException($"Ya existe otra categoría con el nombre '{updateDto.Nombre}'");
                }

            _mapper.Map(updateDto, categoria);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CategoriaDto>(categoria);
        }

        public async Task<bool> DeleteCategoriaAsync(int id, CancellationToken cancellationToken)
        {
            var categoria = await _context.Categorias!
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (categoria == null)
                return false;

            if (_context.Productos != null)
            {
                var hasProducts = await _context.Productos
                    .AnyAsync(p => p.CategoriaId == id, cancellationToken);

                if (hasProducts)
                {
                    throw new InvalidOperationException("No se puede eliminar la categoría porque está siendo utilizada por uno o más productos");
                }
            }

            _context.Categorias!.Remove(categoria);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> CategoriaExistsAsync(string nombre, CancellationToken cancellationToken, int? excludeId = null)
        {
            var query = _context.Categorias!.Where(c => c.Nombre.ToLower() == nombre.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }
    }
}
