using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "El email es requerido.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    [StringLength(320, ErrorMessage = "El email no puede exceder 320 caracteres.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "La contraseña es requerida.")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[\W_]).{8,128}$", 
        ErrorMessage = "La contraseña debe contener al menos una letra mayúscula, una letra minúscula, un número, un carácter especial y tener entre 8 y 128 caracteres.")]
    public required string Password { get; set; }
        
    [Required(ErrorMessage = "El nombre de usuario es requerido.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "El nombre de usuario solo puede contener letras, números y guiones bajos.")]
    public required string UserName { get; set; }
}

