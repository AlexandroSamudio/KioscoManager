using API.DTOs;

namespace API.Interfaces
{
    public interface IConfigRepository
    {
        Task<KioscoConfigDto?> GetKioscoConfigAsync(int kioscoId, CancellationToken cancellationToken);
        Task<Result> UpdateKioscoConfigAsync(int kioscoId, KioscoConfigUpdateDto updateDto, CancellationToken cancellationToken);
        Task<KioscoBasicInfoDto?> GetKioscoBasicInfoAsync(int kioscoId, CancellationToken cancellationToken);
        Task<Result> UpdateKioscoBasicInfoAsync(int kioscoId, KioscoBasicInfoUpdateDto updateDto, CancellationToken cancellationToken);
        Task<UserPreferencesDto?> GetUserPreferencesAsync(int userId, CancellationToken cancellationToken);
        Task<Result> UpdateUserPreferencesAsync(int userId, UserPreferencesUpdateDto updateDto, CancellationToken cancellationToken);
    }
}
