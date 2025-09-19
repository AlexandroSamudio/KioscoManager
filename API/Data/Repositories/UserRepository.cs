using API.Constants;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories;

public class UserRepository(DataContext context, UserManager<AppUser> userManager,
                     RoleManager<AppRole> roleManager, IMapper mapper) : IUserRepository
{
    public async Task<Result<UserManagementDto>> GetUserByIdAsync(int userId,CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null) 
            return Result<UserManagementDto>.Failure(ErrorCodes.EntityNotFound, "Usuario no encontrado");

        var roles = await userManager.GetRolesAsync(user);
        var userDto = mapper.Map<UserManagementDto>(user);
        userDto.Role = roles.FirstOrDefault();

        return Result<UserManagementDto>.Success(userDto);
    }

    public async Task<Result<PagedList<UserManagementDto>>> GetUsersByKioscoAsync(int kioscoId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var kioscoExists = await context.Kioscos.AnyAsync(k => k.Id == kioscoId, cancellationToken);
        if (!kioscoExists)
        {
            return Result<PagedList<UserManagementDto>>.Failure(ErrorCodes.EntityNotFound, "Kiosco no encontrado");
        }

        var query = context.Users
            .Include(u => u.Kiosco)
            .Where(u => u.KioscoId == kioscoId)
            .OrderBy(u => u.UserName);

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userIds = users.Select(u => u.Id).ToList();
        var userRolesDict = await GetUserRolesDictionaryAsync(userIds, cancellationToken);

        var userDtos = users.Select(user =>
        {
            var userDto = mapper.Map<UserManagementDto>(user);
            userDto.Role = userRolesDict.GetValueOrDefault(user.Id);
            userDto.NombreKiosco = user.Kiosco?.Nombre;
            return userDto;
        }).ToList();

        var pagedList = new PagedList<UserManagementDto>(userDtos, totalCount, pageNumber, pageSize);
        return Result<PagedList<UserManagementDto>>.Success(pagedList);
    }

    public async Task<Result<UserRoleResponseDto>> AssignRoleAsync(int userId, string roleName, int requestingUserId, CancellationToken cancellationToken)
    {
        var response = new UserRoleResponseDto
        {
            UserId = userId
        };

        var requestingUser = await context.Users.FindAsync([requestingUserId], cancellationToken);
        if (requestingUser == null)
        {
            return Result<UserRoleResponseDto>.Failure(ErrorCodes.EntityNotFound, "Usuario solicitante no encontrado");
        }

        var isRequestingUserAdmin = await userManager.IsInRoleAsync(requestingUser, "administrador");
        if (!isRequestingUserAdmin)
        {
            return Result<UserRoleResponseDto>.Failure(ErrorCodes.Forbidden, "No tienes permisos para asignar roles");
        }

        if (userId == requestingUserId)
        {
            return Result<UserRoleResponseDto>.Failure(ErrorCodes.Forbidden, "No puedes cambiar tu propio rol");
        }

        var targetUser = await context.Users.FindAsync([userId], cancellationToken);
        if (targetUser == null)
        {
            return Result<UserRoleResponseDto>.Failure(ErrorCodes.EntityNotFound, "Usuario no encontrado");
        }

        if (requestingUser.KioscoId != targetUser.KioscoId)
        {
            return Result<UserRoleResponseDto>.Failure(ErrorCodes.Forbidden, "Solo puedes cambiar roles de usuarios de tu mismo kiosco");
        }

        var roleExists = await roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            return Result<UserRoleResponseDto>.Failure(ErrorCodes.EntityNotFound, $"El rol '{roleName}' no existe");
        }

        var currentRoles = await userManager.GetRolesAsync(targetUser);

        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (currentRoles.Any())
            {
                var removeResult = await userManager.RemoveFromRolesAsync(targetUser, currentRoles);
                if (!removeResult.Succeeded)
                {
                    return Result<UserRoleResponseDto>.Failure(ErrorCodes.InvalidOperation, "Error al remover roles actuales");
                }
            }

            var addResult = await userManager.AddToRoleAsync(targetUser, roleName);
            if (!addResult.Succeeded)
            {
                return Result<UserRoleResponseDto>.Failure(ErrorCodes.InvalidOperation, "Error al asignar nuevo rol");
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<UserRoleResponseDto>.Failure(ErrorCodes.InvalidOperation, "Error al procesar el cambio de rol. La operación ha sido revertida.");
        }
        
        response.UserName = targetUser.UserName ?? "";
        response.Email = targetUser.Email ?? "";
        response.Role = roleName;
        response.Message = $"Rol '{roleName}' asignado exitosamente a {targetUser.UserName}";

        return Result<UserRoleResponseDto>.Success(response, response.Message);
    }

    public async Task<Result<bool>> IsUserAdminAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync([userId], cancellationToken);
        if (user == null) 
            return Result<bool>.Failure(ErrorCodes.EntityNotFound, "Usuario no encontrado");

        var isAdmin = await userManager.IsInRoleAsync(user, "administrador");
        return Result<bool>.Success(isAdmin);
    }

    public async Task<Result> UpdateProfileAsync(int userId, ProfileUpdateDto profileData, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return Result.Failure(ErrorCodes.EntityNotFound, "Usuario no encontrado");
        }

        var existsUserName = await context.Users
            .AnyAsync(u => u.UserName == profileData.UserName && u.Id != userId, cancellationToken);
        if (existsUserName)
        {
            return Result.Failure(ErrorCodes.FieldExists,"El nombre de usuario ya está en uso");
        }

        var existsEmail = await context.Users
            .AnyAsync(u => u.Email == profileData.Email && u.Id != userId, cancellationToken);
        if (existsEmail)
        {
            return Result.Failure(ErrorCodes.FieldExists,"El correo electrónico ya está en uso");
        }

        mapper.Map(profileData, user);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(int userId, ChangePasswordDto passwordData, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync([userId], cancellationToken);
        if (user == null)
        {
            return Result.Failure(ErrorCodes.EntityNotFound, "Usuario no encontrado");
        }

        var passwordCheckResult = await userManager.CheckPasswordAsync(user, passwordData.CurrentPassword!);
        
        if (!passwordCheckResult)
        {
            return Result.Failure(ErrorCodes.InvalidCurrentPassword, "La contraseña actual es incorrecta");
        }

        var changePasswordResult = await userManager.ChangePasswordAsync(user, passwordData.CurrentPassword!, passwordData.NewPassword!);
        if (!changePasswordResult.Succeeded)
        {
            return Result.Failure(ErrorCodes.ValidationError, 
                string.Join(", ", changePasswordResult.Errors.Select(e => e.Description)));
        }

        return Result.Success();
    }

    private async Task<Dictionary<int, string?>> GetUserRolesDictionaryAsync(IList<int> userIds, CancellationToken cancellationToken)
    {
        var userRolesDict = new Dictionary<int, string?>();

        var userRoles = await (from ur in context.UserRoles
                              join r in context.Roles on ur.RoleId equals r.Id
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
