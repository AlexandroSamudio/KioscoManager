namespace API.DTOs;

public enum PasswordChangeErrorCode
{
    None = 0,
    UserNotFound = 1,
    InvalidCurrentPassword = 2,
    PasswordValidationFailed = 3,
    UnknownError = 99
}
