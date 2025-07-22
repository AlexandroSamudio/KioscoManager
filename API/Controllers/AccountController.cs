using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace API.Controllers;

public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService
    , IMapper mapper, DataContext context, ILogger<AccountController> logger) : BaseApiController
{

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.Username)) return BadRequest("El nombre de usuario ya está en uso");


        if (await EmailExists(registerDto.Email)) return BadRequest("El correo electrónico ya está en uso");

        var user = mapper.Map<AppUser>(registerDto);

        var result = await userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded) return BadRequest(result.Errors);

        var roleResult = await userManager.AddToRoleAsync(user, "miembro");
        if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);

        var userDto = mapper.Map<UserDto>(user);
        userDto.Token = await tokenService.CreateToken(user);

        return userDto;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);

        if (user == null) return Unauthorized("Email invalido");

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!result) return Unauthorized("Contraseña invalida");

        var userDto = mapper.Map<UserDto>(user);
        userDto.Token = await tokenService.CreateToken(user);

        return userDto;
    }

    [HttpPost("create-kiosco")]
    public async Task<ActionResult<UserDto>> CreateKiosco(CreateKioscoDto createKioscoDto)
    {
        var userId = User.GetUserId();

        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user == null) return Unauthorized("Usuario no encontrado.");

        if (await userManager.IsInRoleAsync(user, "administrador") || user.KioscoId.HasValue)
        {
            return BadRequest("El usuario ya es un administrador o ya está asignado a un kiosco.");
        }

        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var kiosco = new Kiosco
            {
                Nombre = createKioscoDto.Nombre
            }; context.Kioscos.Add(kiosco);
            await context.SaveChangesAsync();

            var codigoInvitacion = await GenerateAndSaveInvitationCodeAsync(kiosco.Id);

            logger.LogInformation("Código de invitación '{Code}' generado para el kiosco '{KioscoName}' (ID: {KioscoId})",
                codigoInvitacion.Code, kiosco.Nombre, kiosco.Id);

            user.KioscoId = kiosco.Id;

            var removeRoleResult = await userManager.RemoveFromRoleAsync(user, "miembro");
            if (!removeRoleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return BadRequest("Error al remover el rol 'miembro' del usuario.");
            }

            var addRoleResult = await userManager.AddToRoleAsync(user, "administrador");
            if (!addRoleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return BadRequest("Error al añadir el rol 'administrador' al usuario.");
            }

            var updateUserResult = await userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return BadRequest("Error al actualizar el usuario con el nuevo kiosco y roles.");
            }

            await transaction.CommitAsync(); var userDto = mapper.Map<UserDto>(user);
            userDto.Token = await tokenService.CreateToken(user);
            userDto.CodigoInvitacion = codigoInvitacion.Code;

            return userDto;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error en CreateKiosco");
            return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado al procesar la solicitud.");
        }
    }

        
    [HttpGet("kiosco-invitation-codes")]
    public async Task<ActionResult<IEnumerable<object>>> GetKioscoInvitationCodes()
    {
        var userId = User.GetUserId();
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user == null) return Unauthorized("Usuario no encontrado.");

        if (!await userManager.IsInRoleAsync(user, "administrador") || !user.KioscoId.HasValue)
        {
            return BadRequest("Solo los administradores pueden ver los códigos de invitación de su kiosco.");
        }

        var invitationCodes = await context.CodigosInvitacion
            .Where(c => c.KioscoId == user.KioscoId.Value)
            .OrderByDescending(c => c.Id)
            .Select(c => new
            {
                c.Id,
                c.Code,
                c.ExpirationDate,
                c.IsUsed,
                IsExpired = c.ExpirationDate < DateTime.UtcNow
            })
            .ToListAsync();

        return Ok(invitationCodes);
    }

    [HttpPost("join-kiosco")]
    public async Task<ActionResult<UserDto>> JoinKiosco(JoinKioscoDto joinKioscoDto)
    {
        var userId = User.GetUserId();
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user == null) return Unauthorized("Usuario no encontrado.");

        if (user.KioscoId.HasValue)
        {
            return BadRequest("El usuario ya está asignado a un kiosco.");
        }

        var invitationCode = await context.CodigosInvitacion
            .FirstOrDefaultAsync(c => c.Code == joinKioscoDto.CodigoInvitacion);

        if (invitationCode == null)
        {
            return BadRequest("Código de invitación inválido.");
        }

        if (invitationCode.IsUsed)
        {
            return BadRequest("Este código de invitación ya ha sido utilizado.");
        }

        if (invitationCode.ExpirationDate < DateTime.UtcNow)
        {
            return BadRequest("Este código de invitación ha expirado.");
        }

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            user.KioscoId = invitationCode.KioscoId;
            invitationCode.IsUsed = true;

            var updateUserResult = await userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return BadRequest("Error al actualizar el usuario con el nuevo kiosco.");
            }

            context.CodigosInvitacion.Update(invitationCode);
            await context.SaveChangesAsync();

            if (await userManager.IsInRoleAsync(user, "miembro"))
            {
                await userManager.RemoveFromRoleAsync(user, "miembro");
                await userManager.AddToRoleAsync(user, "empleado");
            }

            await transaction.CommitAsync();

            var userDto = mapper.Map<UserDto>(user);
            userDto.Token = await tokenService.CreateToken(user);

            logger.LogInformation("Usuario '{UserName}' (ID: {UserId}) se unió al kiosco ID: {KioscoId} usando el código '{InvitationCode}'",
                user.UserName, user.Id, user.KioscoId, joinKioscoDto.CodigoInvitacion);

            return userDto;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error en JoinKiosco");
            return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado al procesar la solicitud.");
        }
    }


    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("generate-invitation-code")]
    public async Task<ActionResult<GeneratedInvitationCodeDto>> GenerateInvitationCode()
    {
        var userId = User.GetUserId();
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user == null) return Unauthorized("Usuario no encontrado.");
        if (!await userManager.IsInRoleAsync(user, "administrador") || !user.KioscoId.HasValue)
        {
            return BadRequest("Solo los administradores pueden generar códigos de invitación para su kiosco.");
        }

        var newCodigoInvitacion = await GenerateAndSaveInvitationCodeAsync(user.KioscoId.Value);

        logger.LogInformation("Nuevo código de invitación '{Code}' generado por el administrador '{AdminUserName}' (ID: {AdminUserId}) para el kiosco ID: {KioscoId}",
            newCodigoInvitacion.Code, user.UserName, user.Id, user.KioscoId.Value);

        return Ok(new GeneratedInvitationCodeDto
        {
            Code = newCodigoInvitacion.Code,
            ExpirationDate = newCodigoInvitacion.ExpirationDate
        });
    }

    private async Task<CodigoInvitacion> GenerateAndSaveInvitationCodeAsync(int kioscoId)
    {
        string uniqueCode;
        CodigoInvitacion codigoInvitacion;
        int retryAttempts = 0;
        const int maxRetryAttempts = 3;

        do
        {
            uniqueCode = GenerateUniqueInvitationCode();
            var expirationDate = DateTime.UtcNow.AddDays(7);

            codigoInvitacion = new CodigoInvitacion
            {
                Code = uniqueCode,
                KioscoId = kioscoId,
                ExpirationDate = expirationDate,
                IsUsed = false
            };

            try
            {
                context.CodigosInvitacion.Add(codigoInvitacion);
                await context.SaveChangesAsync();
                break;
            }
            catch (DbUpdateException ex) when (retryAttempts < maxRetryAttempts)
            {
                context.CodigosInvitacion.Remove(codigoInvitacion);
                retryAttempts++;

                logger.LogWarning("Violación de índice único detectada al generar código de invitación. Intento {Attempt} de {MaxAttempts}. Error: {Error}",
                    retryAttempts, maxRetryAttempts, ex.Message);

                if (retryAttempts >= maxRetryAttempts)
                {
                    logger.LogError("Se alcanzó el máximo de reintentos ({MaxRetryAttempts}) para generar código de invitación único", maxRetryAttempts);
                    throw new InvalidOperationException("No se pudo generar un código de invitación único después de múltiples intentos.", ex);
                }
            }
        }
        while (retryAttempts < maxRetryAttempts);

        return codigoInvitacion;
    }
    
    private static string GenerateUniqueInvitationCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        const int codeLength = 8;

        return GenerateRandomCode(chars, codeLength);
    }

    private static string GenerateRandomCode(string chars, int length)
    {
        byte[] byteArray = RandomNumberGenerator.GetBytes(length);
        return new string(byteArray.Select(b => chars[b % chars.Length]).ToArray());
    }

    private async Task<bool> UserExists(string username)
    {
        var existingUser = await userManager.FindByNameAsync(username);
        return existingUser != null;
    }

    private async Task<bool> EmailExists(string email)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        return existingUser != null;
    }
}
