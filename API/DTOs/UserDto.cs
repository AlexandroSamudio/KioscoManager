namespace API.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? Token { get; set; }
    public string? UserName { get; set; }
    public int? KioscoId { get; set; }
    public string? CodigoInvitacion { get; set; }
}

public class UserManagementDto
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? Role { get; set; }
    public int? KioscoId { get; set; }
    public string? NombreKiosco { get; set; }
}
public class ProfileUpdateDto
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
}

public class ChangePasswordDto
{
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
}
