using API.DTOs;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class UsersController(IUserRepository _userRepository) : BaseApiController
{
    protected int KioscoId => User.GetKioscoId();
    protected int UserId => User.GetUserId();

    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("{id}")]
    public async Task<ActionResult<UserManagementDto>> GetUser(int id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(id, cancellationToken);

        if (user == null)
        {
            return NotFound("Usuario no encontrado");
        }

        return Ok(user);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("kiosco/{kioscoId}")]
    public async Task<ActionResult<IEnumerable<UserManagementDto>>> GetUsersByKiosco(
        int kioscoId, 
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10, 
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest("Los parámetros de paginación deben ser válidos. PageNumber >= 1, PageSize entre 1 y 100.");
        }

        var users = await _userRepository.GetUsersByKioscoAsync(kioscoId, pageNumber, pageSize, cancellationToken);

        if (users == null || users.Count == 0) return NotFound("No se encontraron usuarios para el kiosco especificado.");

        Response.AddPaginationHeader(users);
        
        return Ok(users);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("{userId}/role")]
    public async Task<ActionResult<UserRoleResponseDto>> AssignRole(int userId, UserRoleAssignmentDto roleAssignment, CancellationToken cancellationToken)
    {
        var result = await _userRepository.AssignRoleAsync(userId, roleAssignment.Role, UserId, cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("{userId}/is-admin")]
    public async Task<ActionResult<bool>> IsUserAdmin(int userId, CancellationToken cancellationToken)
    {
        var isAdmin = await _userRepository.IsUserAdminAsync(userId, cancellationToken);
        return Ok(isAdmin);
    }

    [Authorize]
    [HttpPut("{userId}/perfil")]
    public async Task<IActionResult> UpdateProfile(int userId, ProfileUpdateDto profileData, CancellationToken cancellationToken)
    {
        if (UserId != userId)
        {
            return StatusCode(403, "Solo puedes actualizar tu propio perfil");  
        }

        var result = await _userRepository.UpdateProfileAsync(userId, profileData, cancellationToken);

        return result.ToActionResult();

    }

    [Authorize]
    [HttpPut("{userId}/password")]
    public async Task<IActionResult> ChangePassword(int userId, ChangePasswordDto passwordData, CancellationToken cancellationToken)
    {
        if (UserId != userId)
        {
            return StatusCode(403,"Solo puedes cambiar tu propia contraseña");
        }

        var result = await _userRepository.ChangePasswordAsync(userId, passwordData, cancellationToken);

        return result.ToActionResult();
    }
}
