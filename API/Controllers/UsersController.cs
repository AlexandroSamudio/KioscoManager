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
    public async Task<ActionResult<IEnumerable<UserManagementDto>>> GetUsersPaginated(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10, 
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest("Los parámetros de paginación deben ser válidos. PageNumber >= 1, PageSize entre 1 y 100.");
        }

        var users = await _userRepository.GetUsersAsync(pageNumber, pageSize, cancellationToken);
        
        Response.AddPaginationHeader(users);
        
        return Ok(users);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "administrador")]
    public async Task<ActionResult<UserManagementDto>> GetUser(int id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(id, cancellationToken);

        if (user == null)
        {
            return NotFound("Usuario no encontrado");
        }

        return Ok(user);
    }

    [HttpGet("kiosco/{kioscoId}")]
    [Authorize(Roles = "administrador")]
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
        
        Response.AddPaginationHeader(users);
        
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
    public async Task<ActionResult<IEnumerable<string>>> GetUserRoles(int userId, CancellationToken cancellationToken)
    {
        var roles = await _userRepository.GetUserRolesAsync(userId, cancellationToken);
        return Ok(roles);
    }

    [HttpGet("{userId}/is-admin")]
    [Authorize(Roles = "administrador")]
    public async Task<ActionResult<bool>> IsUserAdmin(int userId, CancellationToken cancellationToken)
    {
        var isAdmin = await _userRepository.IsUserAdminAsync(userId, cancellationToken);
        return Ok(isAdmin);
    }

    /* [HttpPut("{userId}/perfil")]
    [Authorize]
    public async Task<ActionResult<UserManagementDto>> UpdateProfile(int userId, ProfileUpdateDto profileData, CancellationToken cancellationToken)
    {
        var requestingUserId = User.GetUserId();

        if (requestingUserId != userId)
        {
            if (!User.IsInRole("administrador"))
            {
                return Forbid("Solo puedes actualizar tu propio perfil");
            }
        }

        var result = await _userRepository.UpdateProfileAsync(userId, profileData, cancellationToken);

        if (result.IsSuccess && result.Data?.User != null)
        {
            return Ok(result.Data.User);
        }

        var errorResponse = new {
            errorCode = (int)(result.Data?.ErrorCode ?? UpdateEntityErrorCode.UnknownError),
            message = result.Message
        };
        
        return (result.Data?.ErrorCode ?? UpdateEntityErrorCode.UnknownError) switch
        {
            UpdateEntityErrorCode.EntityNotFound => NotFound(errorResponse),
            UpdateEntityErrorCode.FieldExists => BadRequest(errorResponse),
            UpdateEntityErrorCode.EmailExists => BadRequest(errorResponse),
            _ => BadRequest(errorResponse)
        };
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
        
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return result.Data?.ErrorCode switch
        {
            PasswordChangeErrorCode.UserNotFound => NotFound(result.Data),
            PasswordChangeErrorCode.InvalidCurrentPassword => BadRequest(result.Data),
            PasswordChangeErrorCode.PasswordValidationFailed => BadRequest(result.Data),
            _ => BadRequest(result.Data)
        };
    } */
}
