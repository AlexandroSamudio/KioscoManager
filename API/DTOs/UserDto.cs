namespace API.DTOs;

public class UserDto
{
    public required string Email { get; set; }
    public required string Token { get; set; }
    public required string Username { get; set; }
    public int? KioscoId { get; set; }
    public string? CodigoInvitacion { get; set; }
}
