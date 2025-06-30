using System.ComponentModel.DataAnnotations;

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
    public bool NotificacionesStockBajo { get; set; }
    public bool NotificacionesVentas { get; set; }
    public bool NotificacionesReportes { get; set; }
    public string? ConfiguracionesAdicionales { get; set; }
}

public class ChangePasswordDto
{
    [Required]
    public required string CurrentPassword { get; set; }

    [Required]
    [MinLength(6)]
    public required string NewPassword { get; set; }

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden")]
    public required string ConfirmPassword { get; set; }
}
