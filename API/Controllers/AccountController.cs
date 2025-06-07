using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        userDto.Token = tokenService.CreateToken(user);
        
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
        userDto.Token = tokenService.CreateToken(user);
        
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
            userDto.Token = tokenService.CreateToken(user); 

            return userDto;
        }
        catch (Exception ex) 
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error en CreateKiosco");
            return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado al procesar la solicitud.");
        }
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
