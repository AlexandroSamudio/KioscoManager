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

    /// <summary>
    /// Registers a new user with the provided registration details.
    /// </summary>
    /// <param name="registerDto">The registration information for the new user.</param>
    /// <returns>A <see cref="UserDto"/> containing the username, authentication token, and email if registration is successful; otherwise, a bad request with error details.</returns>
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

    /// <summary>
    /// Authenticates a user with the provided email and password.
    /// </summary>
    /// <param name="loginDto">The login credentials containing email and password.</param>
    /// <returns>A <see cref="UserDto"/> with user information and a JWT token if authentication is successful; otherwise, an unauthorized response.</returns>
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

    /// <summary>
    /// Determines whether a user with the specified username exists, using a case-insensitive comparison.
    /// </summary>
    /// <param name="username">The username to check for existence.</param>
    /// <returns>True if a user with the given username exists; otherwise, false.</returns>
    private async Task<bool> UserExists(string username)
    {
        return await userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
    
    /// <summary>
    /// Determines whether a user with the specified email address exists.
    /// </summary>
    /// <param name="email">The email address to check for existence.</param>
    /// <returns>True if a user with the given email exists; otherwise, false.</returns>
    private async Task<bool> EmailExists(string email)
    {
        return await userManager.Users.AnyAsync(x => x.Email == email.ToLower());
    }
}
