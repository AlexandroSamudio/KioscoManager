using API.DTOs;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class AccountController(IAccountRepository accountRepository) : BaseApiController
{
    /// <summary>
    /// Registra un nuevo usuario en el sistema
    /// </summary>
    /// <param name="registerDto">Datos de registro del usuario</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Usuario registrado con token de autenticación</returns>
    /// <response code="200">Usuario registrado exitosamente</response>
    /// <response code="400">Datos de registro inválidos, nombre de usuario o email ya en uso</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto, CancellationToken cancellationToken)
    {
        var result = await accountRepository.RegisterAsync(registerDto, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Autentica un usuario en el sistema
    /// </summary>
    /// <param name="loginDto">Credenciales de acceso (email y contraseña)</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Usuario autenticado con token de acceso</returns>
    /// <response code="200">Usuario autenticado exitosamente</response>
    /// <response code="401">Credenciales inválidas (email o contraseña incorrectos)</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto, CancellationToken cancellationToken)
    {
        var result = await accountRepository.LoginAsync(loginDto, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Crea un nuevo kiosco y asigna al usuario como administrador
    /// </summary>
    /// <param name="createKioscoDto">Datos del kiosco a crear</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Usuario actualizado con rol de administrador y código de invitación del kiosco</returns>
    /// <response code="200">Kiosco creado exitosamente</response>
    /// <response code="400">Datos del kiosco inválidos o error en la creación</response>
    /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
    [Authorize(Policy = "AllowMiembroForKioscoCreation")]
    [HttpPost("create-kiosco")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> CreateKiosco(CreateKioscoDto createKioscoDto, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await accountRepository.CreateKioscoAsync(userId, createKioscoDto, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtiene todos los códigos de invitación del kiosco del usuario autenticado
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Lista de códigos de invitación con su estado</returns>
    /// <response code="200">Lista de códigos obtenida exitosamente</response>
    /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
    /// <response code="400">Usuario no asignado a un kiosco</response>
    [Authorize]
    [HttpGet("kiosco-invitation-codes")]
    [ProducesResponseType(typeof(IReadOnlyList<GeneratedInvitationCodeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<GeneratedInvitationCodeDto>>> GetKioscoInvitationCodes(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await accountRepository.GetKioscoInvitationCodesAsync(userId, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Une al usuario autenticado a un kiosco usando un código de invitación
    /// </summary>
    /// <param name="joinKioscoDto">Código de invitación para unirse al kiosco</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Usuario actualizado con asignación al kiosco</returns>
    /// <response code="200">Usuario unido al kiosco exitosamente</response>
    /// <response code="400">Código inválido, expirado, usado, o usuario ya asignado a un kiosco</response>
    /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
    [Authorize]
    [HttpPost("join-kiosco")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> JoinKiosco(JoinKioscoDto joinKioscoDto, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await accountRepository.JoinKioscoAsync(userId, joinKioscoDto, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Genera un nuevo código de invitación para el kiosco (solo administradores)
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Nuevo código de invitación con fecha de expiración</returns>
    /// <response code="200">Código de invitación generado exitosamente</response>
    /// <response code="400">Error al generar el código o usuario no asignado a un kiosco</response>
    /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
    /// <response code="403">Prohibido. Se requieren permisos de administrador.</response>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("generate-invitation-code")]
    [ProducesResponseType(typeof(GeneratedInvitationCodeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GeneratedInvitationCodeDto>> GenerateInvitationCode(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await accountRepository.GenerateInvitationCodeAsync(userId, cancellationToken);
        return result.ToActionResult();
    }
}
