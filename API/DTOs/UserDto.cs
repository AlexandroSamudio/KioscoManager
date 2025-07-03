using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class UserDto
{
    public required string Email { get; set; }
    public required string Token { get; set; }
    public required string Username { get; set; }
    public int? KioscoId { get; set; }
    public string? CodigoInvitacion { get; set; }
}

public class UserManagementDto
{
    public int Id { get; set; }
    [Required]
    [MaxLength(20, ErrorMessage = "El nombre de usuario no puede exceder 20 caracteres")]
    [MinLength(3, ErrorMessage = "El nombre de usuario debe tener al menos 3 caracteres")]
    public required string UserName { get; set; }
    [Required]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
    public required string Email { get; set; }
    public string? Role { get; set; }
    public int? KioscoId { get; set; }
    public string? NombreKiosco { get; set; }
}

public class ProfileUpdateDto
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
}

public class PasswordChangeResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
