using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    // La contraseña debe tener 1 mayúscula, 1 minúscula, 1 número, 1 carácter especial y tener entre 8 y 128 caracteres de longitud.
    [RegularExpression("^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[\\W_]).{8,128}$", ErrorMessage = "La contraseña debe contener al menos una letra mayúscula, una letra minúscula, un número, un carácter especial y tener entre 8 y 128 caracteres.")]
    [Required]
    public required string Password { get; set; }
        
    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres.")]
    public required string Username { get; set; }
}

