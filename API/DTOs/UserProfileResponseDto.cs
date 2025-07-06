namespace API.DTOs;

public class UserProfileResponseDto
{
    public UpdateProfileErrorCode ErrorCode { get; set; } = UpdateProfileErrorCode.None;
    public UserManagementDto? User { get; set; }
}
