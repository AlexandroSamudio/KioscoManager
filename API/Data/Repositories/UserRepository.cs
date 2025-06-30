using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories;

public class UserRepository : IUserRepository
{
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

    public async Task<UserManagementDto?> GetUserByIdAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Kiosco)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = _mapper.Map<UserManagementDto>(user);
        userDto.Role = roles.FirstOrDefault();
        userDto.NombreKiosco = user.Kiosco?.Nombre;

        return userDto;
    }

    public async Task<IEnumerable<UserManagementDto>> GetUsersAsync()
    {
        var users = await _context.Users
            .Include(u => u.Kiosco)
            .ToListAsync();

        var userDtos = new List<UserManagementDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userDto = _mapper.Map<UserManagementDto>(user);
            userDto.Role = roles.FirstOrDefault();
            userDto.NombreKiosco = user.Kiosco?.Nombre;
            userDtos.Add(userDto);
        }

        return userDtos;
    }

    public async Task<IEnumerable<UserManagementDto>> GetUsersByKioscoAsync(int kioscoId)
    {
        var users = await _context.Users
            .Include(u => u.Kiosco)
            .Where(u => u.KioscoId == kioscoId)
            .ToListAsync();

        var userDtos = new List<UserManagementDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userDto = _mapper.Map<UserManagementDto>(user);
            userDto.Role = roles.FirstOrDefault();
            userDto.NombreKiosco = user.Kiosco?.Nombre;
            userDtos.Add(userDto);
        }

        return userDtos;
    }

    public async Task<UserRoleResponseDto> AssignRoleAsync(int userId, string roleName, int requestingUserId, CancellationToken cancellationToken)
    {
        var response = new UserRoleResponseDto
        {
            UserId = userId,
            Success = false
        };

        var requestingUser = await _context.Users.FindAsync(requestingUserId, cancellationToken);
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

        var targetUser = await _context.Users.FindAsync(userId, cancellationToken);
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

        response.Success = true;
        response.UserName = targetUser.UserName ?? "";
        response.Email = targetUser.Email ?? "";
        response.Role = roleName;
        response.Message = $"Rol '{roleName}' asignado exitosamente a {targetUser.UserName}";

        return response;
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return [];

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<bool> IsUserAdminAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        return await _userManager.IsInRoleAsync(user, "administrador");
    }

    public async Task<UserManagementDto?> UpdateProfileAsync(int userId, ProfileUpdateDto profileData, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Kiosco)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) return null;

        await UpdateUniqueFieldAsync(
            userId,
            profileData.UserName,
            u => u.UserName,
            (u, value) => u.UserName = value,
            "El nombre de usuario ya está en uso",
            cancellationToken
        );

        await UpdateUniqueFieldAsync(
            userId,
            profileData.Email,
            u => u.Email,
            (u, value) => u.Email = value,
            "El correo electrónico ya está en uso",
            cancellationToken
        );

        await _context.SaveChangesAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = _mapper.Map<UserManagementDto>(user);
        userDto.Role = roles.FirstOrDefault();
        userDto.NombreKiosco = user.Kiosco?.Nombre;

        return userDto;
    }

    public async Task<PasswordChangeResponseDto> ChangePasswordAsync(int userId, ChangePasswordDto passwordData, CancellationToken cancellationToken = default)
    {
        var response = new PasswordChangeResponseDto
        {
            Success = false,
            Message = "Error al cambiar la contraseña"
        };

        var user = await _context.Users.FindAsync(userId, cancellationToken);
        if (user == null)
        {
            response.Message = "Usuario no encontrado";
            return response;
        }

        var passwordCheckResult = await _userManager.CheckPasswordAsync(user, passwordData.CurrentPassword);
        if (!passwordCheckResult)
        {
            response.Message = "La contraseña actual es incorrecta";
            return response;
        }

        var changePasswordResult = await _userManager.ChangePasswordAsync(user, passwordData.CurrentPassword, passwordData.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            response.Message = string.Join(", ", changePasswordResult.Errors.Select(e => e.Description));
            return response;
        }

        response.Success = true;
        response.Message = "Contraseña cambiada exitosamente";
        return response;
    }
    
    private async Task UpdateUniqueFieldAsync(
        int userId,
        string? newValue,
        Func<AppUser, string?> selector,
        Action<AppUser, string> setter,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(newValue))
        {
            var existingUser = await _context.Users
                .Where(u => selector(u) == newValue && u.Id != userId)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingUser != null)
            {
                throw new InvalidOperationException(errorMessage);
            }

            var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user != null)
            {
                setter(user, newValue);
            }
        }
    }
}
