using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(UserManager<AppUser> userManager,ITokenService tokenService
    ,IMapper mapper) : BaseApiController
{

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.Username)) return BadRequest("El nombre de usuario ya está en uso");

        
        if (await EmailExists(registerDto.Email)) return BadRequest("El correo electrónico ya está en uso");

        var user = mapper.Map<AppUser>(registerDto);

        var result = await userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded) return BadRequest(result.Errors);

        var roleResult = await userManager.AddToRoleAsync(user, "Empleado");
        
        if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);

        return new UserDto
        {
            Username = user.UserName!,
            Token = tokenService.CreateToken(user),
            Email = user.Email!
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await userManager.Users.SingleOrDefaultAsync(x => x.Email == loginDto.Email.ToLower());

        if (user == null) return Unauthorized("Invalid email");

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!result) return Unauthorized("Invalid password");

        return new UserDto
        {
            Username = user.UserName!,
            Token = tokenService.CreateToken(user),
            Email = user.Email!
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
    
    private async Task<bool> EmailExists(string email)
    {
        return await userManager.Users.AnyAsync(x => x.Email == email.ToLower());
    }
}
