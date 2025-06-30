using API.DTOs;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    [Authorize(Roles = "administrador")]
    public async Task<ActionResult<IEnumerable<UserManagementDto>>> GetUsers()
    {
        var users = await _userRepository.GetUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "administrador")]
    public async Task<ActionResult<UserManagementDto>> GetUser(int id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        
        if (user == null)
        {
            return NotFound("Usuario no encontrado");
        }

        return Ok(user);
    }

    [HttpGet("kiosco/{kioscoId}")]
    [Authorize(Roles = "administrador")]
    public async Task<ActionResult<IEnumerable<UserManagementDto>>> GetUsersByKiosco(int kioscoId)
    {
        var users = await _userRepository.GetUsersByKioscoAsync(kioscoId);
        return Ok(users);
    }

    [HttpPost("{userId}/role")]
    [Authorize(Roles = "administrador")]
    public async Task<ActionResult<UserRoleResponseDto>> AssignRole(int userId, UserRoleAssignmentDto roleAssignment, CancellationToken cancellationToken)
    {
        var requestingUserId = User.GetUserId();
        
        var result = await _userRepository.AssignRoleAsync(userId, roleAssignment.Role, requestingUserId, cancellationToken);
        
        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet("{userId}/roles")]
    [Authorize(Roles = "administrador")]
    public async Task<ActionResult<IEnumerable<string>>> GetUserRoles(int userId)
    {
        var roles = await _userRepository.GetUserRolesAsync(userId);
        return Ok(roles);
    }

    [HttpGet("{userId}/is-admin")]
    [Authorize(Roles = "administrador")]
    public async Task<ActionResult<bool>> IsUserAdmin(int userId)
    {
        var isAdmin = await _userRepository.IsUserAdminAsync(userId);
        return Ok(isAdmin);
    }

    [HttpPut("{userId}/perfil")]
    [Authorize]
    public async Task<ActionResult<UserManagementDto>> UpdateProfile(int userId, ProfileUpdateDto profileData, CancellationToken cancellationToken)
    {
        var requestingUserId = User.GetUserId();
        
        if (requestingUserId != userId)
        {
            var requestingUser = await _userRepository.GetUserByIdAsync(requestingUserId);
            if (requestingUser?.Role != "administrador")
            {
                return Forbid("Solo puedes actualizar tu propio perfil");
            }
        }

        var updatedUser = await _userRepository.UpdateProfileAsync(userId, profileData, cancellationToken);
        
        if (updatedUser == null)
        {
            return NotFound("Usuario no encontrado");
        }

        return Ok(updatedUser);
    }

    [HttpPut("{userId}/password")]
    [Authorize]
    public async Task<ActionResult<PasswordChangeResponseDto>> ChangePassword(int userId, ChangePasswordDto passwordData, CancellationToken cancellationToken)
    {
        var requestingUserId = User.GetUserId();
        
        if (requestingUserId != userId)
        {
            return Forbid("Solo puedes cambiar tu propia contraseña");
        }

        var result = await _userRepository.ChangePasswordAsync(userId, passwordData, cancellationToken);
        
        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
}
