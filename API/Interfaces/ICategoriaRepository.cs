using API.DTOs;
using API.Helpers;

namespace API.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<PagedList<CategoriaDto>> GetCategoriasAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task<CategoriaDto?> GetCategoriaByIdAsync(int id, CancellationToken cancellationToken);
        Task<CategoriaDto> CreateCategoriaAsync(CategoriaCreateDto createDto, CancellationToken cancellationToken);
        Task<CategoriaDto?> UpdateCategoriaAsync(int id, CategoriaUpdateDto updateDto, CancellationToken cancellationToken);
        Task<bool> DeleteCategoriaAsync(int id, CancellationToken cancellationToken);
        Task<bool> CategoriaExistsAsync(string nombre, CancellationToken cancellationToken, int? excludeId = null);
    }
}
