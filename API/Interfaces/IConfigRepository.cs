using API.DTOs;

namespace API.Interfaces
{
    public interface IConfigRepository
    {
        Task<KioscoConfigDto?> GetKioscoConfigAsync(int kioscoId, CancellationToken cancellationToken);
        Task<KioscoConfigDto> UpdateKioscoConfigAsync(int kioscoId, KioscoConfigUpdateDto updateDto, CancellationToken cancellationToken);
        Task<KioscoBasicInfoDto?> GetKioscoBasicInfoAsync(int kioscoId, CancellationToken cancellationToken);
        Task<KioscoBasicInfoDto> UpdateKioscoBasicInfoAsync(int kioscoId, KioscoBasicInfoUpdateDto updateDto, CancellationToken cancellationToken);
        Task<UserPreferencesDto?> GetUserPreferencesAsync(int userId, CancellationToken cancellationToken);
        Task<UserPreferencesDto> UpdateUserPreferencesAsync(int userId, UserPreferencesUpdateDto updateDto, CancellationToken cancellationToken);
    }
}
