using API.DTOs;
using API.Helpers;

namespace API.Interfaces;

public interface IUserRepository
{
    Task<UserManagementDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken);
    Task<PagedList<UserManagementDto>> GetUsersByKioscoAsync(int kioscoId, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<Result<UserRoleResponseDto>> AssignRoleAsync(int userId, string roleName, int requestingUserId, CancellationToken cancellationToken);
    Task<bool> IsUserAdminAsync(int userId, CancellationToken cancellationToken);
    Task<Result> UpdateProfileAsync(int userId, ProfileUpdateDto profileData, CancellationToken cancellationToken);
    Task<Result> ChangePasswordAsync(int userId, ChangePasswordDto passwordData, CancellationToken cancellationToken);
}
