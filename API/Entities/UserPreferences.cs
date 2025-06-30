namespace API.Entities;

public class UserPreferences
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public bool NotificacionesStockBajo { get; set; } = true;

    public bool NotificacionesVentas { get; set; } = false;

    public bool NotificacionesReportes { get; set; } = false;
    public string? ConfiguracionesAdicionales { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public static UserPreferences CreateDefault(int userId)
    {
        return new UserPreferences
        {
            UserId = userId
        };
    }
}
