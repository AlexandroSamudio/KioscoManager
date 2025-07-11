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

    public async Task<PagedList<UserManagementDto>> GetUsersByKioscoAsync(int kioscoId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        // Crear una consulta base que filtre por kioscoId
        var query = _context.Users
            .Include(u => u.Kiosco)
            .Where(u => u.KioscoId == kioscoId)
            .OrderBy(u => u.UserName); // Ordenar por nombre de usuario para una presentación consistente

        // Obtener el conteo total de usuarios que pertenecen a este kiosco
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplicar paginación a la consulta
        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Obtener los roles para todos los usuarios de la página actual
        var userIds = users.Select(u => u.Id).ToList();
        var userRolesDict = await GetUserRolesDictionaryAsync(userIds, cancellationToken);

        // Mapear las entidades de usuarios a DTOs y asignar información adicional
        var userDtos = users.Select(user =>
        {
            var userDto = _mapper.Map<UserManagementDto>(user);
            userDto.Role = userRolesDict.GetValueOrDefault(user.Id);
            userDto.NombreKiosco = user.Kiosco?.Nombre;
            return userDto;
        }).ToList();

        // Devolver una lista paginada con los DTOs de usuarios y metadatos de paginación
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
        response.Username = targetUser.UserName ?? "";
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

    /* public async Task<Result<UserProfileResponseDto>> UpdateProfileAsync(int userId, ProfileUpdateDto profileData, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            var notFoundResponse = new UserProfileResponseDto
            {
                ErrorCode = UpdateEntityErrorCode.EntityNotFound
            };
            return Result<UserProfileResponseDto>.Failure(notFoundResponse, notFoundResponse.ErrorCode.ToString(), "Usuario no encontrado");
        }

        var userNameResult = await ValidateUniqueFieldAsync(user, profileData.UserName, UserNameField, cancellationToken);
        if (!userNameResult.IsSuccess)
        {
            var usernameExistsResponse = new UserProfileResponseDto
            {
                ErrorCode = UpdateEntityErrorCode.FieldExists
            };
            return Result<UserProfileResponseDto>.Failure(usernameExistsResponse, usernameExistsResponse.ErrorCode.ToString(), "El nombre de usuario ya está en uso");
        }

        var emailResult = await ValidateUniqueFieldAsync(user, profileData.Email, EmailField, cancellationToken);
        if (!emailResult.IsSuccess)
        {
            var emailExistsResponse = new UserProfileResponseDto
            {
                ErrorCode = UpdateEntityErrorCode.EmailExists
            };
            return Result<UserProfileResponseDto>.Failure(emailExistsResponse, emailExistsResponse.ErrorCode.ToString(), "El correo electrónico ya está en uso");
        }

        await _context.SaveChangesAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = _mapper.Map<UserManagementDto>(user);
        userDto.Role = roles.FirstOrDefault();

        var successResponse = new UserProfileResponseDto
        {
            ErrorCode = UpdateEntityErrorCode.None,
            User = userDto
        };
        
        return Result<UserProfileResponseDto>.Success(successResponse, "Perfil actualizado correctamente");
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
            return Result<PasswordChangeResponseDto>.Failure(notFoundResponse,notFoundResponse.ErrorCode.ToString() ,"Usuario no encontrado");
        }

        var passwordCheckResult = await _userManager.CheckPasswordAsync(user, passwordData.CurrentPassword);
        if (!passwordCheckResult)
        {
            var invalidPasswordResponse = new PasswordChangeResponseDto
            {
                ErrorCode = PasswordChangeErrorCode.InvalidCurrentPassword
            };
            return Result<PasswordChangeResponseDto>.Failure(invalidPasswordResponse, invalidPasswordResponse.ErrorCode.ToString(), "La contraseña actual es incorrecta");
        }

        var changePasswordResult = await _userManager.ChangePasswordAsync(user, passwordData.CurrentPassword, passwordData.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            var errorMessage = string.Join(", ", changePasswordResult.Errors.Select(e => e.Description));
            var validationFailedResponse = new PasswordChangeResponseDto
            {
                ErrorCode = PasswordChangeErrorCode.PasswordValidationFailed
            };
            return Result<PasswordChangeResponseDto>.Failure(validationFailedResponse, validationFailedResponse.ErrorCode.ToString(), errorMessage);
        }

        var successResponse = new PasswordChangeResponseDto
        {
            ErrorCode = PasswordChangeErrorCode.None
        };
        
        return Result<PasswordChangeResponseDto>.Success(successResponse, "Contraseña cambiada exitosamente");
    }
    
    private async Task<Result> ValidateUniqueFieldAsync(AppUser user, string? newValue, string fieldName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(newValue))
        {
            return Result.Success();
        }

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
            return Result.Failure(string.Empty, $"Campo no soportado: {fieldName}");
        }

        var existingUser = await fieldConfig.QueryFunc(_context.Users);
        if (existingUser != null)
        {
            return Result.Failure(string.Empty);
        }

        fieldConfig.UpdateAction(newValue);
        return Result.Success();
    } */

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
