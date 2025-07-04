using API.DTOs;
using API.Helpers;

namespace API.Interfaces;

public interface IUserRepository
{
    Task<UserManagementDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken);
    Task<PagedList<UserManagementDto>> GetUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<IEnumerable<UserManagementDto>> GetUsersByKioscoAsync(int kioscoId, CancellationToken cancellationToken);
    Task<UserRoleResponseDto> AssignRoleAsync(int userId, string roleName, int requestingUserId, CancellationToken cancellationToken);
    Task<IEnumerable<string>> GetUserRolesAsync(int userId, CancellationToken cancellationToken);
    Task<bool> IsUserAdminAsync(int userId, CancellationToken cancellationToken);
    Task<UserManagementDto?> UpdateProfileAsync(int userId, ProfileUpdateDto profileData, CancellationToken cancellationToken);
    Task<PasswordChangeResponseDto> ChangePasswordAsync(int userId, ChangePasswordDto passwordData, CancellationToken cancellationToken);
}
