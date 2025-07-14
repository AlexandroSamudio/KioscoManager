using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public class ConfigRepository(DataContext context, IMapper mapper) : IConfigRepository
    {
        private readonly DataContext _context = context;
        private readonly IMapper _mapper = mapper;

        public async Task<KioscoConfigDto?> GetKioscoConfigAsync(int kioscoId, CancellationToken cancellationToken)
        {
            var kioscoConfig = await _context.KioscoConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(kc => kc.KioscoId == kioscoId, cancellationToken);

            kioscoConfig ??= await _context.EnsureKioscoConfigAsync(kioscoId);

            return _mapper.Map<KioscoConfigDto>(kioscoConfig);
        }

        public async Task<Result> UpdateKioscoConfigAsync(int kioscoId, KioscoConfigUpdateDto updateDto, CancellationToken cancellationToken)
        {
            var kioscoConfig = await _context.KioscoConfigs
                .FirstOrDefaultAsync(kc => kc.KioscoId == kioscoId, cancellationToken);

            if (kioscoConfig == null) return Result.Failure("Configuración del kiosco no encontrada");
            
            _mapper.Map(updateDto, kioscoConfig);
            kioscoConfig.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<KioscoBasicInfoDto?> GetKioscoBasicInfoAsync(int kioscoId, CancellationToken cancellationToken)
        {
            var kiosco = await _context.Kioscos
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.Id == kioscoId, cancellationToken);

            return kiosco == null ? null : _mapper.Map<KioscoBasicInfoDto>(kiosco);
        }

        public async Task<Result> UpdateKioscoBasicInfoAsync(int kioscoId, KioscoBasicInfoUpdateDto updateDto, CancellationToken cancellationToken)
        {
            var kiosco = await _context.Kioscos
                .FirstOrDefaultAsync(k => k.Id == kioscoId, cancellationToken);

            if (kiosco == null) return Result.Failure("Kiosco no encontrado");

            _mapper.Map(updateDto, kiosco);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<UserPreferencesDto?> GetUserPreferencesAsync(int userId, CancellationToken cancellationToken)
        {
            var userPreferences = await _context.UserPreferences
                .AsNoTracking()
                .FirstOrDefaultAsync(up => up.UserId == userId, cancellationToken);

            if (userPreferences == null)
            {
                userPreferences = await _context.EnsureUserPreferencesAsync(userId);
            }

            return _mapper.Map<UserPreferencesDto>(userPreferences);
        }

        public async Task<Result> UpdateUserPreferencesAsync(int userId, UserPreferencesUpdateDto updateDto, CancellationToken cancellationToken)
        {
            var userPreferences = await _context.UserPreferences
                .FirstOrDefaultAsync(up => up.UserId == userId, cancellationToken);

            if (userPreferences == null) return Result.Failure("Preferencias del usuario no encontradas");

            _mapper.Map(updateDto, userPreferences);

            userPreferences.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
