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
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Role { get; set; }
    public int? KioscoId { get; set; }
    public string? NombreKiosco { get; set; }
}

public class ProfileUpdateDto
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
}

public class PasswordChangeDto
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmNewPassword { get; set; }
}

public class PasswordChangeResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
