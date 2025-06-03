using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    // La contraseña debe tener 1 mayúscula, 1 minúscula, 1 número y tener entre 4 y 8 caracteres de longitud.
    [RegularExpression("(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{4,8}$", ErrorMessage = "La contraseña debe cumplir con los requisitos de complejidad: 1 mayúscula, 1 minúscula, 1 número, 4-8 caracteres.")]
    public required string Password { get; set; }
    
    [Required]
    public required string Username { get; set; }
}

