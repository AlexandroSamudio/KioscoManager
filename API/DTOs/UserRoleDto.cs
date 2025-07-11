using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class UserRoleAssignmentDto
{
    [Required]
    public required string Role { get; set; }
}

public class UserRoleResponseDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
