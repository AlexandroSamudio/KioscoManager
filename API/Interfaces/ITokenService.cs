using API.Entities;

namespace API.Interfaces;

public interface ITokenService
{
    /// <summary>
/// Generates a token for the specified user.
/// </summary>
/// <param name="user">The user for whom the token is generated.</param>
/// <returns>A token string representing the user.</returns>
string CreateToken(AppUser user);
}
