using API.Entities;
using API.Helpers;
using API.DTOs;

namespace API.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<PagedList<CategoriaDto>> GetCategoriasAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task<CategoriaDto?> GetCategoriaByIdAsync(int id, CancellationToken cancellationToken);
        Task<Result<CategoriaDto>> CreateCategoriaAsync(CategoriaCreateDto createDto, CancellationToken cancellationToken);
        Task<Result> UpdateCategoriaAsync(int id, CategoriaUpdateDto updateDto, CancellationToken cancellationToken);
        Task<Result> DeleteCategoriaAsync(int id, CancellationToken cancellationToken);
        Task<bool> CategoriaExistsAsync(string nombre, CancellationToken cancellationToken);
    }
}
