using API.Data;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ConfigurationExtensions
    {
        public static async Task<KioscoConfig> EnsureKioscoConfigAsync(this DataContext context, int kioscoId)
        {
            var existingConfig = await context.KioscoConfigs
                .FirstOrDefaultAsync(kc => kc.KioscoId == kioscoId);

            if (existingConfig != null)
                return existingConfig;

            var newConfig = KioscoConfig.CreateDefault(kioscoId);
            context.KioscoConfigs.Add(newConfig);
            await context.SaveChangesAsync();

            return newConfig;
        }

        public static async Task<UserPreferences> EnsureUserPreferencesAsync(this DataContext context, int userId)
        {
            var existingPreferences = await context.UserPreferences
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (existingPreferences != null)
                return existingPreferences;

            var newPreferences = UserPreferences.CreateDefault(userId);
            context.UserPreferences.Add(newPreferences);
            await context.SaveChangesAsync();

            return newPreferences;
        }

        public static async Task EnsureAllKioscoConfigsAsync(this DataContext context)
        {
            var kioscosWithoutConfig = await context.Kioscos
                .Include(k => k.Configuracion)
                .Where(k => k.Configuracion == null)
                .ToListAsync();

            foreach (var kiosco in kioscosWithoutConfig)
            {
                var config = KioscoConfig.CreateDefault(kiosco.Id);
                context.KioscoConfigs.Add(config);
            }

            if (kioscosWithoutConfig.Any())
            {
                await context.SaveChangesAsync();
            }
        }

        public static async Task EnsureAllUserPreferencesAsync(this DataContext context)
        {
            var usersWithoutPreferences = await context.Users
                .Include(u => u.Preferencias)
                .Where(u => u.Preferencias == null)
                .ToListAsync();

            foreach (var user in usersWithoutPreferences)
            {
                var preferences = UserPreferences.CreateDefault(user.Id);
                context.UserPreferences.Add(preferences);
            }

            if (usersWithoutPreferences.Any())
            {
                await context.SaveChangesAsync();
            }
        }
    }
}
