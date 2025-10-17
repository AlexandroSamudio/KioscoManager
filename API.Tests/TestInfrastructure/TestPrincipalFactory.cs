using System.Security.Claims;

namespace API.Tests.TestInfrastructure;

public static class TestPrincipalFactory
{
  public static ClaimsPrincipal Create(int? kioscoId = null, int? userId = null, params string[] roles)
  {
    var claims = new List<Claim>();
    if (kioscoId.HasValue)
    {
      claims.Add(new Claim("kioscoId", kioscoId.Value.ToString()));
    }
    if (userId.HasValue)
    {
      claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
    }
    if (roles is { Length: > 0 })
    {
      claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
    }

    var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
    return new ClaimsPrincipal(identity);
  }
}
