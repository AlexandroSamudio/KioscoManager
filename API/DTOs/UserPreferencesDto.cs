namespace API.DTOs;

public class UserPreferencesDto
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public bool NotificacionesStockBajo { get; set; }
    public bool NotificacionesVentas { get; set; }
    public bool NotificacionesReportes { get; set; }
    public string? ConfiguracionesAdicionales { get; set; }
    public DateTime FechaActualizacion { get; set; }
}

public class UserPreferencesUpdateDto
{
    public bool? NotificacionesStockBajo { get; set; }
    public bool? NotificacionesVentas { get; set; }
    public bool? NotificacionesReportes { get; set; }
    public string? ConfiguracionesAdicionales { get; set; }
}
