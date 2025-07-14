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
    private readonly DataContext _context = context;
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly RoleManager<AppRole> _roleManager = roleManager;
    private readonly IMapper _mapper = mapper;

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

    public async Task<PagedList<UserManagementDto>> GetUsersByKioscoAsync(int kioscoId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Users
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
            var userDto = _mapper.Map<UserManagementDto>(user);
            userDto.Role = userRolesDict.GetValueOrDefault(user.Id);
            userDto.NombreKiosco = user.Kiosco?.Nombre;
            return userDto;
        }).ToList();

        return new PagedList<UserManagementDto>(userDtos, totalCount, pageNumber, pageSize);
    }

    public async Task<Result<UserRoleResponseDto>> AssignRoleAsync(int userId, string roleName, int requestingUserId, CancellationToken cancellationToken)
    {
        var response = new UserRoleResponseDto
        {
            UserId = userId,
            Success = false
        };

        var requestingUser = await _context.Users.FindAsync([requestingUserId], cancellationToken);
        if (requestingUser == null)
        {
            return Result<UserRoleResponseDto>.Failure(ErrorCodes.EntityNotFound, "Usuario solicitante no encontrado");
        }

        var isRequestingUserAdmin = await _userManager.IsInRoleAsync(requestingUser, "administrador");
        if (!isRequestingUserAdmin)
        {
            return Result<UserRoleResponseDto>.Failure(ErrorCodes.Forbidden, "No tienes permisos para asignar roles");
        }

        if (userId == requestingUserId)
        {
            return Result<UserRoleResponseDto>.Failure(ErrorCodes.Forbidden, "No puedes cambiar tu propio rol");
        }

        var targetUser = await _context.Users.FindAsync([userId], cancellationToken);
        if (targetUser == null)
        {
            return Result<UserRoleResponseDto>.Failure(ErrorCodes.EntityNotFound, "Usuario objetivo no encontrado");
        }

        if (requestingUser.KioscoId != targetUser.KioscoId)
        {
            return Result<UserRoleResponseDto>.Failure(ErrorCodes.Forbidden, "Solo puedes cambiar roles de usuarios de tu mismo kiosco");
        }

        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            return Result<UserRoleResponseDto>.Failure(ErrorCodes.EntityNotFound, $"El rol '{roleName}' no existe");
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
                    return Result<UserRoleResponseDto>.Failure(ErrorCodes.InvalidOperation, "Error al remover roles actuales");
                }
            }

            var addResult = await _userManager.AddToRoleAsync(targetUser, roleName);
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

        response.Success = true;
        response.Username = targetUser.UserName ?? "";
        response.Email = targetUser.Email ?? "";
        response.Role = roleName;
        response.Message = $"Rol '{roleName}' asignado exitosamente a {targetUser.UserName}";

        return Result<UserRoleResponseDto>.Success(response, response.Message);
    }

    public async Task<bool> IsUserAdminAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync([userId], cancellationToken);
        if (user == null) return false;

        return await _userManager.IsInRoleAsync(user, "administrador");
    }

    public async Task<Result> UpdateProfileAsync(int userId, ProfileUpdateDto profileData, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return Result.Failure(ErrorCodes.EntityNotFound, "Usuario no encontrado");
        }

        var existsUserName = await _context.Users
            .AnyAsync(u => u.UserName == profileData.UserName && u.Id != userId, cancellationToken);
        if (existsUserName)
        {
            return Result.Failure(ErrorCodes.FieldExists,"El nombre de usuario ya está en uso");
        }

        var existsEmail = await _context.Users
            .AnyAsync(u => u.Email == profileData.Email && u.Id != userId, cancellationToken);
        if (existsEmail)
        {
            return Result.Failure(ErrorCodes.FieldExists,"El correo electrónico ya está en uso");
        }
        
        user.UserName = profileData.UserName;
        user.Email = profileData.Email;


        await _context.SaveChangesAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = _mapper.Map<UserManagementDto>(user);
        userDto.Role = roles.FirstOrDefault();

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(int userId, ChangePasswordDto passwordData, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync([userId], cancellationToken);
        if (user == null)
        {
            return Result.Failure(ErrorCodes.EntityNotFound, "Usuario no encontrado");
        }

        var passwordCheckResult = await _userManager.CheckPasswordAsync(user, passwordData.CurrentPassword);
        
        if (!passwordCheckResult)
        {
            return Result.Failure(ErrorCodes.InvalidCurrentPassword, "La contraseña actual es incorrecta");
        }

        var changePasswordResult = await _userManager.ChangePasswordAsync(user, passwordData.CurrentPassword, passwordData.NewPassword);
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
