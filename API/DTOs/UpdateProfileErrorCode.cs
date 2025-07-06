namespace API.DTOs;

public enum UpdateProfileErrorCode
{
    None = 0,
    UserNotFound = 1,
    UsernameExists = 2,
    EmailExists = 3,
    UnknownError = 99
}
