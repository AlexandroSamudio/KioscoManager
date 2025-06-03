using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace API.Data.SeedData
{
    public static class JsonDataSeeder
    {
        public static async Task SeedAsync<T>(DataContext context, string jsonFileName, ILogger logger) where T : class
        {
            var dbSet = context.Set<T>();

            if (await dbSet.AnyAsync())
            {
                logger.LogInformation($"La tabla para {typeof(T).Name} ya contiene datos. No se realizará el seeding.");
                return;
            }

            var filePath = Path.Combine("Data", "SeedData", "JSONFiles", jsonFileName);

            if (!File.Exists(filePath))
            {
                logger.LogError($"El archivo de seed {filePath} para la entidad {typeof(T).Name} no fue encontrado.");
                return;
            }

            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var entities = JsonSerializer.Deserialize<List<T>>(json, options);

                if (entities != null && entities.Any())
                {
                    await dbSet.AddRangeAsync(entities);
                    await context.SaveChangesAsync();
                    logger.LogInformation($"Seeding para {typeof(T).Name} completado desde {jsonFileName}. {entities.Count} entidades añadidas.");
                }
                else
                {
                    logger.LogWarning($"No se encontraron entidades en {jsonFileName} para {typeof(T).Name} o el archivo está vacío.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Ocurrió un error durante el seeding de {typeof(T).Name} desde {jsonFileName}.");
            }
        }
    }
}
