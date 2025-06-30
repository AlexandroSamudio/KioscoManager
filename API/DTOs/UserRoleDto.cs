namespace API.DTOs;

public class UserRoleAssignmentDto
{
    public string Role { get; set; } = string.Empty;
}

public class UserRoleResponseDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
