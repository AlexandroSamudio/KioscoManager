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
    public class CategoriaRepository(DataContext context, IMapper mapper) : ICategoriaRepository
    {
        public async Task<PagedList<CategoriaDto>> GetCategoriasAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            var query = context.Categorias!
                .AsNoTracking()
                .OrderBy(c => c.Nombre)
                .ProjectTo<CategoriaDto>(mapper.ConfigurationProvider);

            return await PagedList<CategoriaDto>.CreateAsync(query, pageNumber, pageSize, cancellationToken);
        }

        public async Task<CategoriaDto?> GetCategoriaByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await context.Categorias!
                .AsNoTracking()
                .Where(c => c.Id == id)
                .ProjectTo<CategoriaDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Result<CategoriaDto>> CreateCategoriaAsync(CategoriaCreateDto createDto, CancellationToken cancellationToken)
        {
            if (await CategoriaExistsAsync(createDto.Nombre, cancellationToken))
            {
                return Result<CategoriaDto>.Failure(ErrorCodes.FieldExists, $"Ya existe una categoría con el nombre '{createDto.Nombre}'");
            }

            var categoria = mapper.Map<Categoria>(createDto);

            context.Categorias!.Add(categoria);
            await context.SaveChangesAsync(cancellationToken);

            var categoriaDto = mapper.Map<CategoriaDto>(categoria);
            return Result<CategoriaDto>.Success(categoriaDto);
        }

        public async Task<Result> UpdateCategoriaAsync(int id, CategoriaUpdateDto updateDto, CancellationToken cancellationToken)
        {
            var categorias = await context.Categorias!
                .Where(c => c.Id == id || c.Nombre.Equals(updateDto.Nombre, StringComparison.CurrentCultureIgnoreCase))
                .ToListAsync(cancellationToken);

            var categoria = categorias.FirstOrDefault(c => c.Id == id);
            if (categoria == null)
                return Result.Failure(ErrorCodes.EntityNotFound, "Categoría no encontrada");

            if (categorias.Any(c => c.Id != id && c.Nombre.Equals(updateDto.Nombre, StringComparison.CurrentCultureIgnoreCase)))
                return Result.Failure(ErrorCodes.FieldExists, $"Ya existe una categoría con el nombre '{updateDto.Nombre}'");

            mapper.Map(updateDto, categoria);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<Result> DeleteCategoriaAsync(int id, CancellationToken cancellationToken)
        {
            var categoria = await context.Categorias!
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (categoria == null)
                return Result.Failure(ErrorCodes.EntityNotFound, "Categoría no encontrada");

            if (context.Productos != null)
            {
                var hasProducts = await context.Productos
                    .AnyAsync(p => p.CategoriaId == id, cancellationToken);

                if (hasProducts)
                {
                    return Result.Failure(ErrorCodes.InvalidOperation, "No se puede eliminar la categoría porque tiene productos asociados.");
                }
            }

            context.Categorias!.Remove(categoria);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<bool> CategoriaExistsAsync(string nombre, CancellationToken cancellationToken)
        {
            var query = context.Categorias!.Where(c => c.Nombre.Equals(nombre, StringComparison.CurrentCultureIgnoreCase));

            return await query.AnyAsync(cancellationToken);
        }
    }
}
