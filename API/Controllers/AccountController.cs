using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(UserManager<AppUser> userManager,ITokenService tokenService
    ,IMapper mapper, DataContext context,ILogger<AccountController> logger) : BaseApiController
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
            };

            context.Kioscos.Add(kiosco);
            await context.SaveChangesAsync();           
            var uniqueCode = await GenerateUniqueInvitationCodeAsync();
            var codigoInvitacion = new CodigoInvitacion
            {
                Code = uniqueCode,
                KioscoId = kiosco.Id,
                ExpirationDate = DateTime.UtcNow.AddDays(7),
                IsUsed = false
            };

            context.CodigosInvitacion.Add(codigoInvitacion);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Código de invitación '{Code}' generado para el kiosco '{KioscoName}' (ID: {KioscoId})", 
                uniqueCode, kiosco.Nombre, kiosco.Id);

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

            await transaction.CommitAsync();

            var userDto = mapper.Map<UserDto>(user);
            userDto.Token = await tokenService.CreateToken(user); 
            userDto.CodigoInvitacion = uniqueCode;

            return userDto;
        }
        catch (Exception ex) 
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error en CreateKiosco");
            return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado al procesar la solicitud.");
        }
    }    
    private async Task<string> GenerateUniqueInvitationCodeAsync()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        const int codeLength = 8;
        string code;
        int attempts = 0;
        const int maxAttempts = 10;

        do
        {
            attempts++;
            code = GenerateRandomCode(chars, codeLength);
            
            if (attempts > maxAttempts)
            {
                logger.LogWarning("Se alcanzó el máximo de intentos ({MaxAttempts}) para generar un código único", maxAttempts);
                code = $"{code}{DateTime.UtcNow.Ticks % 1000:D3}";
                break;
            }
        }
        while (await context.CodigosInvitacion.AnyAsync(c => c.Code == code));

        return code;
    }

    private static string GenerateRandomCode(string chars, int length)
    {
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
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

        var uniqueCode = await GenerateUniqueInvitationCodeAsync();
        var expirationDate = DateTime.UtcNow.AddDays(7);

        var newCodigoInvitacion = new CodigoInvitacion
        {
            Code = uniqueCode,
            KioscoId = user.KioscoId.Value,
            ExpirationDate = expirationDate,
            IsUsed = false
        };

        context.CodigosInvitacion.Add(newCodigoInvitacion);
        await context.SaveChangesAsync();

        logger.LogInformation("Nuevo código de invitación '{Code}' generado por el administrador '{AdminUserName}' (ID: {AdminUserId}) para el kiosco ID: {KioscoId}",
            uniqueCode, user.UserName, user.Id, user.KioscoId.Value);

        return Ok(new GeneratedInvitationCodeDto
        {
            Code = newCodigoInvitacion.Code,
            ExpirationDate = newCodigoInvitacion.ExpirationDate
        });
    }
}
