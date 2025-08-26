using System.ComponentModel.DataAnnotations;
using API.Attributes;

namespace API.DTOs;

public class UserDto
{
    public int Id { get; set; }
    [Required]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
    public required string Email { get; set; }
    [Required]
    public required string Token { get; set; }
    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres.")]
    public required string UserName { get; set; }
    public int? KioscoId { get; set; }
    public string? CodigoInvitacion { get; set; }
}

public class UserManagementDto
{
    public int Id { get; set; }
    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres.")]
    public required string UserName { get; set; }
    [Required]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
    public required string Email { get; set; }
    public string? Role { get; set; }
    public int? KioscoId { get; set; }
    public string? NombreKiosco { get; set; }
}

[AtLeastOneProperty(ErrorMessage = "Se debe proporcionar al menos un campo para actualizar.")]
public class ProfileUpdateDto
{
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres.")]
    public string? UserName { get; set; }
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
    public string? Email { get; set; }
}

public class ChangePasswordDto
{
    [Required]
    public required string CurrentPassword { get; set; }

    [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[\W_]).{8,128}$", ErrorMessage = "La contraseña debe contener al menos una letra mayúscula, una letra minúscula, un número, un carácter especial y tener entre 8 y 128 caracteres.")]
    [Required]
    public required string NewPassword { get; set; }

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden")]
    public required string ConfirmPassword { get; set; }
}
