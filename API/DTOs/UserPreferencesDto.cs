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
    public bool? NotificacionesStockBajo { get; set; }
    public bool? NotificacionesVentas { get; set; }
    public bool? NotificacionesReportes { get; set; }
    public string? ConfiguracionesAdicionales { get; set; }
}

public class ChangePasswordDto
{
    [Required]
    public required string CurrentPassword { get; set; }

    // La contraseña debe tener 1 mayúscula, 1 minúscula, 1 número, 1 carácter especial y tener entre 8 y 128 caracteres de longitud.
    [RegularExpression("^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[\\W_]).{8,128}$", ErrorMessage = "La contraseña debe contener al menos una letra mayúscula, una letra minúscula, un número, un carácter especial y tener entre 8 y 128 caracteres.")]
    [Required]
    public required string NewPassword { get; set; }

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden")]
    public required string ConfirmPassword { get; set; }
}
