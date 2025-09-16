using System.ComponentModel.DataAnnotations;
using API.Constants;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository) : BaseApiController
{
    protected int KioscoId => User.GetKioscoId();
    protected int UserId => User.GetUserId();

    /// <summary>
    /// Obtiene la información de un usuario específico
    /// </summary>
    /// <param name="id">ID del usuario a consultar</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Información detallada del usuario</returns>
    /// <response code="200">Usuario obtenido exitosamente</response>
    /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
    /// <response code="403">Prohibido. Se requieren permisos de administrador.</response>
    /// <response code="404">Usuario no encontrado</response>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserManagementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserManagementDto>> GetUser(int id, CancellationToken cancellationToken)
    {
        var result = await userRepository.GetUserByIdAsync(id, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtiene la lista paginada de usuarios de un kiosco específico
    /// </summary>
    /// <param name="kioscoId">ID del kiosco para filtrar usuarios</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <param name="pageNumber">Número de página (por defecto: 1)</param>
    /// <param name="pageSize">Tamaño de página (por defecto: 10, máximo: 10)</param>
    /// <returns>Lista paginada de usuarios del kiosco</returns>
    /// <response code="200">Lista de usuarios obtenida exitosamente</response>
    /// <response code="400">Parámetros de paginación inválidos</response>
    /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
    /// <response code="403">Prohibido. Se requieren permisos de administrador.</response>
    /// <response code="404">Kiosco no encontrado</response>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("kiosco/{kioscoId}")]
    [ProducesResponseType(typeof(PagedList<UserManagementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedList<UserManagementDto>>> GetUsersByKiosco(
        int kioscoId,
        CancellationToken cancellationToken,
        [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery, Range(1, 10)] int pageSize = 10)
    {
        var result = await userRepository.GetUsersByKioscoAsync(kioscoId, pageNumber, pageSize, cancellationToken);

        return result.ToActionResult(users =>
        {
            Response.AddPaginationHeader(users);
            return Ok(users);
        });
    }

    /// <summary>
    /// Asigna un rol específico a un usuario
    /// </summary>
    /// <param name="userId">ID del usuario al que se asignará el rol</param>
    /// <param name="roleAssignment">Datos del rol a asignar</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Confirmación de la asignación del rol</returns>
    /// <response code="200">Rol asignado exitosamente</response>
    /// <response code="400">Datos de asignación de rol inválidos</response>
    /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
    /// <response code="403">Prohibido. Se requieren permisos de administrador o no puedes cambiar tu propio rol.</response>
    /// <response code="404">Usuario o rol no encontrado</response>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("{userId}/role")]
    [ProducesResponseType(typeof(UserRoleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserRoleResponseDto>> AssignRole(int userId, UserRoleAssignmentDto roleAssignment, CancellationToken cancellationToken)
    {
        var result = await userRepository.AssignRoleAsync(userId, roleAssignment.Role, UserId, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Verifica si un usuario tiene permisos de administrador
    /// </summary>
    /// <param name="userId">ID del usuario a verificar</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>True si el usuario es administrador, false en caso contrario</returns>
    /// <response code="200">Verificación completada exitosamente</response>
    /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
    /// <response code="403">Prohibido. Se requieren permisos de administrador.</response>
    /// <response code="404">Usuario no encontrado</response>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("{userId}/is-admin")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> IsUserAdmin(int userId, CancellationToken cancellationToken)
    {
        var result = await userRepository.IsUserAdminAsync(userId, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Actualiza el perfil del usuario autenticado
    /// </summary>
    /// <param name="userId">ID del usuario (debe coincidir con el usuario autenticado)</param>
    /// <param name="profileData">Nuevos datos del perfil</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Confirmación de la actualización del perfil</returns>
    /// <response code="204">Perfil actualizado exitosamente</response>
    /// <response code="400">Datos del perfil inválidos o nombre de usuario/email ya en uso</response>
    /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
    /// <response code="403">Prohibido. Solo puedes actualizar tu propio perfil.</response>
    /// <response code="404">Usuario no encontrado</response>
    [Authorize]
    [HttpPut("{userId}/perfil")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile(int userId, ProfileUpdateDto profileData, CancellationToken cancellationToken)
    {
        if (UserId != userId)
        {
            return Result.Failure(ErrorCodes.Forbidden, "Solo puedes actualizar tu propio perfil").ToActionResult();  
        }

        var result = await userRepository.UpdateProfileAsync(userId, profileData, cancellationToken);

        return result.ToActionResult();

    }

    /// <summary>
    /// Cambia la contraseña del usuario autenticado
    /// </summary>
    /// <param name="userId">ID del usuario (debe coincidir con el usuario autenticado)</param>
    /// <param name="passwordData">Datos para el cambio de contraseña (contraseña actual y nueva)</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Confirmación del cambio de contraseña</returns>
    /// <response code="204">Contraseña cambiada exitosamente</response>
    /// <response code="400">Datos de contraseña inválidos o contraseña actual incorrecta</response>
    /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
    /// <response code="403">Prohibido. Solo puedes actualizar tu propio perfil.</response>
    /// <response code="404">Usuario no encontrado</response>
    [Authorize]
    [HttpPut("{userId}/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(int userId, ChangePasswordDto passwordData, CancellationToken cancellationToken)
    {
        if (UserId != userId)
        {
            return Result.Failure(ErrorCodes.Forbidden, "Solo puedes cambiar tu propia contraseña").ToActionResult();
        }

        var result = await userRepository.ChangePasswordAsync(userId, passwordData, cancellationToken);

        return result.ToActionResult();
    }
}
