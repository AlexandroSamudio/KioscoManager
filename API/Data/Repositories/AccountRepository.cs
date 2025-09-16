using API.Constants;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace API.Data.Repositories;

public class AccountRepository(DataContext context, UserManager<AppUser> userManager, 
    ITokenService tokenService, IMapper mapper) : IAccountRepository
{
    public async Task<Result<UserDto>> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken)
    {
        if (await UserExistsAsync(registerDto.UserName, cancellationToken))
            return Result<UserDto>.Failure(ErrorCodes.FieldExists, "El nombre de usuario ya está en uso");

        if (await EmailExistsAsync(registerDto.Email, cancellationToken))
            return Result<UserDto>.Failure(ErrorCodes.FieldExists, "El correo electrónico ya está en uso");

        var user = mapper.Map<AppUser>(registerDto);

        var result = await userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
            return Result<UserDto>.Failure(ErrorCodes.ValidationError, 
                string.Join(", ", result.Errors.Select(e => e.Description)));

        var roleResult = await userManager.AddToRoleAsync(user, "miembro");
        if (!roleResult.Succeeded)
            return Result<UserDto>.Failure(ErrorCodes.InvalidOperation, 
                string.Join(", ", roleResult.Errors.Select(e => e.Description)));

        var userDto = mapper.Map<UserDto>(user);
        userDto.Token = await tokenService.CreateToken(user);

        return Result<UserDto>.Success(userDto);
    }

    public async Task<Result<UserDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
            return Result<UserDto>.Failure(ErrorCodes.InvalidCredentials, "Email inválido");

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!result)
            return Result<UserDto>.Failure(ErrorCodes.InvalidCredentials, "Contraseña inválida");

        var userDto = mapper.Map<UserDto>(user);
        userDto.Token = await tokenService.CreateToken(user);

        return Result<UserDto>.Success(userDto);
    }

    public async Task<Result<UserDto>> CreateKioscoAsync(int userId, CreateKioscoDto createKioscoDto, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result<UserDto>.Failure(ErrorCodes.EntityNotFound, "Usuario no encontrado");

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var kiosco = new Kiosco
            {
                Nombre = createKioscoDto.Nombre!
            };
            
            context.Kioscos.Add(kiosco);
            await context.SaveChangesAsync(cancellationToken);

            var codigoInvitacionResult = await GenerateAndSaveInvitationCodeAsync(kiosco.Id, cancellationToken);
            if (!codigoInvitacionResult.IsSuccess)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<UserDto>.Failure(codigoInvitacionResult.ErrorCode!, codigoInvitacionResult.Message);
            }

            user.KioscoId = kiosco.Id;

            if (await userManager.IsInRoleAsync(user, "miembro"))
            {
                var removeRoleResult = await userManager.RemoveFromRoleAsync(user, "miembro");
                if (!removeRoleResult.Succeeded)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<UserDto>.Failure(ErrorCodes.InvalidOperation, "Error al remover el rol 'miembro' del usuario");
                }
            }

            var addRoleResult = await userManager.AddToRoleAsync(user, "administrador");
            if (!addRoleResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<UserDto>.Failure(ErrorCodes.InvalidOperation, "Error al añadir el rol 'administrador' al usuario");
            }

            var updateUserResult = await userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<UserDto>.Failure(ErrorCodes.InvalidOperation, "Error al actualizar el usuario con el nuevo kiosco y roles");
            }

            await transaction.CommitAsync(cancellationToken);

            var userDto = mapper.Map<UserDto>(user);
            userDto.Token = await tokenService.CreateToken(user);
            userDto.CodigoInvitacion = codigoInvitacionResult.Data!.Code;

            return Result<UserDto>.Success(userDto);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<UserDto>.Failure(ErrorCodes.InvalidOperation, "Ocurrió un error inesperado al crear el kiosco");
        }
    }

    public async Task<Result<IReadOnlyList<GeneratedInvitationCodeDto>>> GetKioscoInvitationCodesAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result<IReadOnlyList<GeneratedInvitationCodeDto>>.Failure(ErrorCodes.EntityNotFound, "Usuario no encontrado");

        if (!user.KioscoId.HasValue)
            return Result<IReadOnlyList<GeneratedInvitationCodeDto>>.Failure(ErrorCodes.InvalidOperation, "El usuario no está asignado a un kiosco");

        var invitationCodes = await context.CodigosInvitacion
            .Where(c => c.KioscoId == user.KioscoId.Value)
            .OrderByDescending(c => c.Id)
            .Select(c => new GeneratedInvitationCodeDto
            {
                Code = c.Code,
                ExpirationDate = c.ExpirationDate
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GeneratedInvitationCodeDto>>.Success(invitationCodes);
    }

    public async Task<Result<UserDto>> JoinKioscoAsync(int userId, JoinKioscoDto joinKioscoDto, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result<UserDto>.Failure(ErrorCodes.EntityNotFound, "Usuario no encontrado");

        if (user.KioscoId.HasValue)
            return Result<UserDto>.Failure(ErrorCodes.InvalidOperation, "El usuario ya está asignado a un kiosco");

        var invitationCode = await context.CodigosInvitacion
            .FirstOrDefaultAsync(c => c.Code == joinKioscoDto.CodigoInvitacion, cancellationToken);

        if (invitationCode == null)
            return Result<UserDto>.Failure(ErrorCodes.EntityNotFound, "Código de invitación inválido");

        if (invitationCode.IsUsed)
            return Result<UserDto>.Failure(ErrorCodes.InvalidOperation, "Este código de invitación ya ha sido utilizado");

        if (invitationCode.ExpirationDate < DateTime.UtcNow)
            return Result<UserDto>.Failure(ErrorCodes.InvalidOperation, "Este código de invitación ha expirado");

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            user.KioscoId = invitationCode.KioscoId;
            invitationCode.IsUsed = true;

            var updateUserResult = await userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<UserDto>.Failure(ErrorCodes.InvalidOperation, "Error al actualizar el usuario con el nuevo kiosco");
            }

            context.CodigosInvitacion.Update(invitationCode);
            await context.SaveChangesAsync(cancellationToken);

            if (await userManager.IsInRoleAsync(user, "miembro"))
            {
                var removeResult = await userManager.RemoveFromRoleAsync(user, "miembro");
                if (!removeResult.Succeeded)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<UserDto>.Failure(ErrorCodes.InvalidOperation, "Error al remover el rol 'miembro'");
                }
                
                var addResult = await userManager.AddToRoleAsync(user, "empleado");
                if (!addResult.Succeeded)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<UserDto>.Failure(ErrorCodes.InvalidOperation, "Error al añadir el rol 'empleado'");
                }
            }

            await transaction.CommitAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            var userDto = mapper.Map<UserDto>(user);
            userDto.Token = await tokenService.CreateToken(user);

            return Result<UserDto>.Success(userDto);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<UserDto>.Failure(ErrorCodes.InvalidOperation, "Ocurrió un error inesperado al unirse al kiosco");
        }
    }

    public async Task<Result<GeneratedInvitationCodeDto>> GenerateInvitationCodeAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result<GeneratedInvitationCodeDto>.Failure(ErrorCodes.EntityNotFound, "Usuario no encontrado");

        if (!user.KioscoId.HasValue)
            return Result<GeneratedInvitationCodeDto>.Failure(ErrorCodes.InvalidOperation, "El usuario no está asignado a un kiosco");

        var codigoInvitacionResult = await GenerateAndSaveInvitationCodeAsync(user.KioscoId.Value, cancellationToken);
        if (!codigoInvitacionResult.IsSuccess)
            return Result<GeneratedInvitationCodeDto>.Failure(codigoInvitacionResult.ErrorCode!, codigoInvitacionResult.Message);

        var generatedCodeDto = new GeneratedInvitationCodeDto
        {
            Code = codigoInvitacionResult.Data!.Code,
            ExpirationDate = codigoInvitacionResult.Data.ExpirationDate
        };

        return Result<GeneratedInvitationCodeDto>.Success(generatedCodeDto);
    }

    private async Task<Result<CodigoInvitacion>> GenerateAndSaveInvitationCodeAsync(int kioscoId, CancellationToken cancellationToken)
    {
        int retryAttempts = 0;
        const int maxRetryAttempts = 3;

        do
        {
            var uniqueCode = GenerateUniqueInvitationCode();
            var expirationDate = DateTime.UtcNow.AddDays(7);

            var codigoInvitacion = new CodigoInvitacion
            {
                Code = uniqueCode,
                KioscoId = kioscoId,
                ExpirationDate = expirationDate,
                IsUsed = false
            };

            try
            {
                context.CodigosInvitacion.Add(codigoInvitacion);
                await context.SaveChangesAsync(cancellationToken);
                return Result<CodigoInvitacion>.Success(codigoInvitacion);
            }
            catch (DbUpdateException) when (retryAttempts < maxRetryAttempts)
            {
                context.CodigosInvitacion.Remove(codigoInvitacion);
                retryAttempts++;

                if (retryAttempts >= maxRetryAttempts)
                {
                    return Result<CodigoInvitacion>.Failure(ErrorCodes.InvalidOperation,
                        "No se pudo generar un código de invitación único después de múltiples intentos");
                }
            }
        }
        while (retryAttempts < maxRetryAttempts);

        return Result<CodigoInvitacion>.Failure(ErrorCodes.InvalidOperation,
            "Error inesperado al generar código de invitación");
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

    private async Task<bool> UserExistsAsync(string username, CancellationToken cancellationToken)
    {
        var existingUser = await userManager.FindByNameAsync(username);
        return existingUser != null;
    }

    private async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        return existingUser != null;
    }
}
