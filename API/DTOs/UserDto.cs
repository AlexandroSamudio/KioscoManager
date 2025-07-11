using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class UserDto
{
    public int Id { get; set; }
    [Required]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
    public required string Email { get; set; }
    public required string Token { get; set; }
    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres.")]
    public required string Username { get; set; }
    public int? KioscoId { get; set; }
    public string? CodigoInvitacion { get; set; }
}

public class UserManagementDto
{
    public int Id { get; set; }
    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres.")]
    public required string Username { get; set; }
    [Required]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
    public required string Email { get; set; }
    public string? Role { get; set; }
    public int? KioscoId { get; set; }
    public string? NombreKiosco { get; set; }
}

public class ProfileUpdateDto
{
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres.")]
    public string? UserName { get; set; }
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
    public string? Email { get; set; }
}

public class PasswordChangeResponseDto
{
    public PasswordChangeErrorCode ErrorCode { get; set; } = PasswordChangeErrorCode.None;
}
