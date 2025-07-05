using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories;

public class UserRepository : IUserRepository
{
    private const string UserNameField = "UserName";
    private const string EmailField = "Email";

    private readonly DataContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IMapper _mapper;

    public UserRepository(DataContext context, UserManager<AppUser> userManager,
                         RoleManager<AppRole> roleManager, IMapper mapper)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
    }

    public async Task<UserManagementDto?> GetUserByIdAsync(int userId,CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = _mapper.Map<UserManagementDto>(user);
        userDto.Role = roles.FirstOrDefault();

        return userDto;
    }

    public async Task<PagedList<UserManagementDto>> GetUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Users
            .OrderBy(u => u.Id);

        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userIds = users.Select(u => u.Id).ToList();
        var userRolesDict = await GetUserRolesDictionaryAsync(userIds, cancellationToken);

        var userDtos = users.Select(user =>
        {
            var userDto = _mapper.Map<UserManagementDto>(user);
            userDto.Role = userRolesDict.GetValueOrDefault(user.Id);
            return userDto;
        }).ToList();

        return new PagedList<UserManagementDto>(userDtos, totalCount, pageNumber, pageSize);
    }

    public async Task<IEnumerable<UserManagementDto>> GetUsersByKioscoAsync(int kioscoId, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .Include(u => u.Kiosco)
            .Where(u => u.KioscoId == kioscoId)
            .ToListAsync(cancellationToken);

        var userIds = users.Select(u => u.Id).ToList();
        var userRolesDict = await GetUserRolesDictionaryAsync(userIds, cancellationToken);

        var userDtos = users.Select(user =>
        {
            var userDto = _mapper.Map<UserManagementDto>(user);
            userDto.Role = userRolesDict.GetValueOrDefault(user.Id);
            userDto.NombreKiosco = user.Kiosco?.Nombre;
            return userDto;
        }).ToList();

        return userDtos;
    }

    public async Task<UserRoleResponseDto> AssignRoleAsync(int userId, string roleName, int requestingUserId, CancellationToken cancellationToken)
    {
        var response = new UserRoleResponseDto
        {
            UserId = userId,
            Success = false
        };

        var requestingUser = await _context.Users.FindAsync([requestingUserId], cancellationToken);
        if (requestingUser == null)
        {
            response.Message = "Usuario solicitante no encontrado";
            return response;
        }

        var isRequestingUserAdmin = await _userManager.IsInRoleAsync(requestingUser, "administrador");
        if (!isRequestingUserAdmin)
        {
            response.Message = "No tienes permisos para asignar roles";
            return response;
        }

        if (userId == requestingUserId)
        {
            response.Message = "No puedes cambiar tu propio rol";
            return response;
        }

        var targetUser = await _context.Users.FindAsync([userId], cancellationToken);
        if (targetUser == null)
        {
            response.Message = "Usuario no encontrado";
            return response;
        }

        if (requestingUser.KioscoId != targetUser.KioscoId)
        {
            response.Message = "Solo puedes cambiar roles de usuarios de tu mismo kiosco";
            return response;
        }

        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            response.Message = $"El rol '{roleName}' no existe";
            return response;
        }

        var currentRoles = await _userManager.GetRolesAsync(targetUser);

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(targetUser, currentRoles);
                if (!removeResult.Succeeded)
                {
                    response.Message = "Error al remover roles actuales";
                    return response;
                }
            }

            var addResult = await _userManager.AddToRoleAsync(targetUser, roleName);
            if (!addResult.Succeeded)
            {
                response.Message = $"Error al asignar el rol: {string.Join(", ", addResult.Errors.Select(e => e.Description))}";
                return response;
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            response.Message = "Error al procesar el cambio de rol. La operación ha sido revertida.";
            return response;
        }

        response.Success = true;
        response.UserName = targetUser.UserName ?? "";
        response.Email = targetUser.Email ?? "";
        response.Role = roleName;
        response.Message = $"Rol '{roleName}' asignado exitosamente a {targetUser.UserName}";

        return response;
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync([userId], cancellationToken);
        if (user == null) return [];

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<bool> IsUserAdminAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync([userId], cancellationToken);
        if (user == null) return false;

        return await _userManager.IsInRoleAsync(user, "administrador");
    }

    public async Task<UserManagementDto?> UpdateProfileAsync(int userId, ProfileUpdateDto profileData, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) return null;

        await ValidateUniqueFieldAsync(user, profileData.UserName, UserNameField, "El nombre de usuario ya está en uso", cancellationToken);
        await ValidateUniqueFieldAsync(user, profileData.Email, EmailField, "El correo electrónico ya está en uso", cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = _mapper.Map<UserManagementDto>(user);
        userDto.Role = roles.FirstOrDefault();

        return userDto;
    }

    public async Task<Result<PasswordChangeResponseDto>> ChangePasswordAsync(int userId, ChangePasswordDto passwordData, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync([userId], cancellationToken);
        if (user == null)
        {
            var notFoundResponse = new PasswordChangeResponseDto
            {
                ErrorCode = PasswordChangeErrorCode.UserNotFound
            };
            return Result<PasswordChangeResponseDto>.Failure(notFoundResponse, "USER_NOT_FOUND", "Usuario no encontrado");
        }

        var passwordCheckResult = await _userManager.CheckPasswordAsync(user, passwordData.CurrentPassword);
        if (!passwordCheckResult)
        {
            var invalidPasswordResponse = new PasswordChangeResponseDto
            {
                ErrorCode = PasswordChangeErrorCode.InvalidCurrentPassword
            };
            return Result<PasswordChangeResponseDto>.Failure(invalidPasswordResponse, "INVALID_CURRENT_PASSWORD", "La contraseña actual es incorrecta");
        }

        var changePasswordResult = await _userManager.ChangePasswordAsync(user, passwordData.CurrentPassword, passwordData.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            var errorMessage = string.Join(", ", changePasswordResult.Errors.Select(e => e.Description));
            var validationFailedResponse = new PasswordChangeResponseDto
            {
                ErrorCode = PasswordChangeErrorCode.PasswordValidationFailed
            };
            return Result<PasswordChangeResponseDto>.Failure(validationFailedResponse, "PASSWORD_VALIDATION_FAILED", errorMessage);
        }

        var successResponse = new PasswordChangeResponseDto
        {
            ErrorCode = PasswordChangeErrorCode.None
        };
        
        return Result<PasswordChangeResponseDto>.Success(successResponse, "Contraseña cambiada exitosamente");
    }
    
    private async Task ValidateUniqueFieldAsync(AppUser user, string? newValue, string fieldName, string errorMessage, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(newValue)) return;

        var fieldConfigs = new Dictionary<string, (Func<IQueryable<AppUser>, Task<AppUser?>> QueryFunc, Action<string> UpdateAction)>
        {
            [UserNameField] = (
                async query => await query.Where(u => u.UserName == newValue && u.Id != user.Id).FirstOrDefaultAsync(cancellationToken),
                value => user.UserName = value
            ),
            [EmailField] = (
                async query => await query.Where(u => u.Email == newValue && u.Id != user.Id).FirstOrDefaultAsync(cancellationToken),
                value => user.Email = value
            )
        };

        if (!fieldConfigs.TryGetValue(fieldName, out var fieldConfig))
        {
            throw new ArgumentException($"Campo no soportado: {fieldName}", nameof(fieldName));
        }

        var existingUser = await fieldConfig.QueryFunc(_context.Users);
        if (existingUser != null)
        {
            throw new InvalidOperationException(errorMessage);
        }

        fieldConfig.UpdateAction(newValue);
    }

    private async Task<Dictionary<int, string?>> GetUserRolesDictionaryAsync(IList<int> userIds, CancellationToken cancellationToken)
    {
        var userRolesDict = new Dictionary<int, string?>();

        var userRoles = await (from ur in _context.UserRoles
                              join r in _context.Roles on ur.RoleId equals r.Id
                              where userIds.Contains(ur.UserId)
                              select new { ur.UserId, r.Name })
                              .ToListAsync(cancellationToken);

        var userRoleGroups = userRoles.GroupBy(ur => ur.UserId);
        
        foreach (var group in userRoleGroups)
        {
            userRolesDict[group.Key] = group.FirstOrDefault()?.Name;
        }

        foreach (var userId in userIds)
        {
            if (!userRolesDict.ContainsKey(userId))
            {
                userRolesDict[userId] = null;
            }
        }

        return userRolesDict;
    }
}
