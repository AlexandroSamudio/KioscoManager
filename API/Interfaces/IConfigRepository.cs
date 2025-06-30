using API.DTOs;

namespace API.Interfaces
{
    public interface IConfigRepository
    {
        Task<KioscoConfigDto?> GetKioscoConfigAsync(int kioscoId, CancellationToken cancellationToken = default);
        Task<KioscoConfigDto> UpdateKioscoConfigAsync(int kioscoId, KioscoConfigUpdateDto updateDto, CancellationToken cancellationToken = default);
        Task<KioscoBasicInfoDto?> GetKioscoBasicInfoAsync(int kioscoId, CancellationToken cancellationToken = default);
        Task<KioscoBasicInfoDto> UpdateKioscoBasicInfoAsync(int kioscoId, KioscoBasicInfoUpdateDto updateDto, CancellationToken cancellationToken = default);
        Task<UserPreferencesDto?> GetUserPreferencesAsync(int userId, CancellationToken cancellationToken = default);
        Task<UserPreferencesDto> UpdateUserPreferencesAsync(int userId, UserPreferencesUpdateDto updateDto, CancellationToken cancellationToken = default);
    }
}
