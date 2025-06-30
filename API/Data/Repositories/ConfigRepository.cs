using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public class ConfigRepository : IConfigRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public ConfigRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<KioscoConfigDto?> GetKioscoConfigAsync(int kioscoId, CancellationToken cancellationToken = default)
        {
            var kioscoConfig = await _context.KioscoConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(kc => kc.KioscoId == kioscoId, cancellationToken);

            if (kioscoConfig == null)
            {
                kioscoConfig = await _context.EnsureKioscoConfigAsync(kioscoId);
            }

            return _mapper.Map<KioscoConfigDto>(kioscoConfig);
        }

        public async Task<KioscoConfigDto> UpdateKioscoConfigAsync(int kioscoId, KioscoConfigUpdateDto updateDto, CancellationToken cancellationToken = default)
        {
            var kioscoConfig = await _context.KioscoConfigs
                .FirstOrDefaultAsync(kc => kc.KioscoId == kioscoId, cancellationToken);

            if (kioscoConfig == null)
            {
                throw new InvalidOperationException("Configuración del kiosco no encontrada");
            }

            _mapper.Map(updateDto, kioscoConfig);
            kioscoConfig.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<KioscoConfigDto>(kioscoConfig);
        }

        public async Task<KioscoBasicInfoDto?> GetKioscoBasicInfoAsync(int kioscoId, CancellationToken cancellationToken = default)
        {
            var kiosco = await _context.Kioscos
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.Id == kioscoId, cancellationToken);

            return kiosco == null ? null : _mapper.Map<KioscoBasicInfoDto>(kiosco);
        }

        public async Task<KioscoBasicInfoDto> UpdateKioscoBasicInfoAsync(int kioscoId, KioscoBasicInfoUpdateDto updateDto, CancellationToken cancellationToken = default)
        {
            var kiosco = await _context.Kioscos
                .FirstOrDefaultAsync(k => k.Id == kioscoId, cancellationToken);

            if (kiosco == null)
            {
                throw new InvalidOperationException("Kiosco no encontrado");
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Nombre))
                kiosco.Nombre = updateDto.Nombre;
            
            if (updateDto.Direccion != null)
                kiosco.Direccion = updateDto.Direccion;
            
            if (updateDto.Telefono != null)
                kiosco.Telefono = updateDto.Telefono;

            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<KioscoBasicInfoDto>(kiosco);
        }

        public async Task<UserPreferencesDto?> GetUserPreferencesAsync(int userId, CancellationToken cancellationToken = default)
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

        public async Task<UserPreferencesDto> UpdateUserPreferencesAsync(int userId, UserPreferencesUpdateDto updateDto, CancellationToken cancellationToken = default)
        {
            var userPreferences = await _context.UserPreferences
                .FirstOrDefaultAsync(up => up.UserId == userId, cancellationToken);

            if (userPreferences == null)
            {
                throw new InvalidOperationException("Preferencias del usuario no encontradas");
            }

            _mapper.Map(updateDto, userPreferences);
            userPreferences.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<UserPreferencesDto>(userPreferences);
        }
    }
}
