using API.DTOs;
using API.Helpers;

namespace API.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<PagedList<CategoriaDto>> GetCategoriasAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        Task<CategoriaDto?> GetCategoriaByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<CategoriaDto> CreateCategoriaAsync(CategoriaCreateDto createDto, CancellationToken cancellationToken = default);
        Task<CategoriaDto?> UpdateCategoriaAsync(int id, CategoriaUpdateDto updateDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteCategoriaAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> CategoriaExistsAsync(string nombre, int? excludeId = null, CancellationToken cancellationToken = default);
    }
}
