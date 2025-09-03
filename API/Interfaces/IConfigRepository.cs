using API.DTOs;
using API.Entities;

namespace API.Interfaces
{
    public interface IConfigRepository
    {
        Task<Result<KioscoConfigDto>> GetKioscoConfigAsync(int kioscoId, CancellationToken cancellationToken);
        Task<Result<KioscoConfigDto>> UpdateKioscoConfigAsync(int kioscoId, KioscoConfigUpdateDto updateDto, CancellationToken cancellationToken);
        Task<Result<KioscoBasicInfoDto>> GetKioscoBasicInfoAsync(int kioscoId, CancellationToken cancellationToken);
        Task<Result<KioscoBasicInfoDto>> UpdateKioscoBasicInfoAsync(int kioscoId, KioscoBasicInfoUpdateDto updateDto, CancellationToken cancellationToken);
        Task<Result<UserPreferencesDto>> GetUserPreferencesAsync(int userId, CancellationToken cancellationToken);
        Task<Result<UserPreferencesDto>> UpdateUserPreferencesAsync(int userId, UserPreferencesUpdateDto updateDto, CancellationToken cancellationToken);
    }
}
