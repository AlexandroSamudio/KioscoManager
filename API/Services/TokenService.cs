using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService : ITokenService
{
    private readonly SymmetricSecurityKey _key;
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenService"/> class using the token key from configuration.
    /// </summary>
    /// <param name="config">Configuration containing the "TokenKey" setting used for JWT signing.</param>
    /// <exception cref="InvalidOperationException">Thrown if the "TokenKey" configuration value is missing or empty.</exception>
    public TokenService(IConfiguration config)
    {
        var tokenKey = config["TokenKey"];
        if (string.IsNullOrEmpty(tokenKey))
        {
            throw new InvalidOperationException("La TokenKey no ha sido configurada.");
        }
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
    }

    /// <summary>
    /// Generates a JWT token containing the user's ID and email as claims.
    /// </summary>
    /// <param name="user">The user for whom the token is generated.</param>
    /// <returns>A signed JWT token string valid for 7 days.</returns>
    public string CreateToken(AppUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
