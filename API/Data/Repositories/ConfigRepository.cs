using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using API.Constants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public class ConfigRepository(DataContext context, IMapper mapper) : IConfigRepository
    {
        public async Task<Result<KioscoConfigDto>> GetKioscoConfigAsync(int kioscoId, CancellationToken cancellationToken)
        {
            var kioscoConfig = await context.KioscoConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(kc => kc.KioscoId == kioscoId, cancellationToken);

            kioscoConfig ??= await context.EnsureKioscoConfigAsync(kioscoId);

            var configDto = mapper.Map<KioscoConfigDto>(kioscoConfig);
            return Result<KioscoConfigDto>.Success(configDto);
        }

        public async Task<Result<KioscoConfigDto>> UpdateKioscoConfigAsync(int kioscoId, KioscoConfigUpdateDto updateDto, CancellationToken cancellationToken)
        {
            var kioscoConfig = await context.KioscoConfigs
                .FirstOrDefaultAsync(kc => kc.KioscoId == kioscoId, cancellationToken);

            if (kioscoConfig == null) 
                return Result<KioscoConfigDto>.Failure(ErrorCodes.EntityNotFound, "Configuración del kiosco no encontrada");
            
            mapper.Map(updateDto, kioscoConfig);
            kioscoConfig.FechaActualizacion = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            var configDto = mapper.Map<KioscoConfigDto>(kioscoConfig);
            return Result<KioscoConfigDto>.Success(configDto);
        }

        public async Task<Result<KioscoBasicInfoDto>> GetKioscoBasicInfoAsync(int kioscoId, CancellationToken cancellationToken)
        {
            var kiosco = await context.Kioscos
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.Id == kioscoId, cancellationToken);

            if (kiosco == null)
                return Result<KioscoBasicInfoDto>.Failure(ErrorCodes.EntityNotFound, "Kiosco no encontrado");

            var kioscoDto = mapper.Map<KioscoBasicInfoDto>(kiosco);
            return Result<KioscoBasicInfoDto>.Success(kioscoDto);
        }

        public async Task<Result<KioscoBasicInfoDto>> UpdateKioscoBasicInfoAsync(int kioscoId, KioscoBasicInfoUpdateDto updateDto, CancellationToken cancellationToken)
        {
            var kiosco = await context.Kioscos
                .FirstOrDefaultAsync(k => k.Id == kioscoId, cancellationToken);

            if (kiosco == null) 
                return Result<KioscoBasicInfoDto>.Failure(ErrorCodes.EntityNotFound, "Kiosco no encontrado");

            mapper.Map(updateDto, kiosco);

            await context.SaveChangesAsync(cancellationToken);

            var kioscoDto = mapper.Map<KioscoBasicInfoDto>(kiosco);
            return Result<KioscoBasicInfoDto>.Success(kioscoDto);
        }

        public async Task<Result<UserPreferencesDto>> GetUserPreferencesAsync(int userId, CancellationToken cancellationToken)
        {
            var userPreferences = await context.UserPreferences
                .AsNoTracking()
                .FirstOrDefaultAsync(up => up.UserId == userId, cancellationToken);

            if (userPreferences == null)
            {
                userPreferences = await context.EnsureUserPreferencesAsync(userId);
            }

            var preferencesDto = mapper.Map<UserPreferencesDto>(userPreferences);
            return Result<UserPreferencesDto>.Success(preferencesDto);
        }

        public async Task<Result<UserPreferencesDto>> UpdateUserPreferencesAsync(int userId, UserPreferencesUpdateDto updateDto, CancellationToken cancellationToken)
        {
            var userPreferences = await context.UserPreferences
                .FirstOrDefaultAsync(up => up.UserId == userId, cancellationToken);

            if (userPreferences == null) 
                return Result<UserPreferencesDto>.Failure(ErrorCodes.EntityNotFound, "Preferencias del usuario no encontradas");

            mapper.Map(updateDto, userPreferences);

            userPreferences.FechaActualizacion = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            var preferencesDto = mapper.Map<UserPreferencesDto>(userPreferences);
            return Result<UserPreferencesDto>.Success(preferencesDto);
        }
    }
}
