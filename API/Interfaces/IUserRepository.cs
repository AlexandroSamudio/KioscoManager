using API.DTOs;

namespace API.Interfaces;

public interface IUserRepository
{
    Task<UserManagementDto?> GetUserByIdAsync(int userId);
    Task<IEnumerable<UserManagementDto>> GetUsersAsync();
    Task<IEnumerable<UserManagementDto>> GetUsersByKioscoAsync(int kioscoId);
    Task<UserRoleResponseDto> AssignRoleAsync(int userId, string roleName, int requestingUserId, CancellationToken cancellationToken);
    Task<IEnumerable<string>> GetUserRolesAsync(int userId);
    Task<bool> IsUserAdminAsync(int userId);
    Task<UserManagementDto?> UpdateProfileAsync(int userId, ProfileUpdateDto profileData, CancellationToken cancellationToken);
    Task<PasswordChangeResponseDto> ChangePasswordAsync(int userId, ChangePasswordDto passwordData, CancellationToken cancellationToken);
}
