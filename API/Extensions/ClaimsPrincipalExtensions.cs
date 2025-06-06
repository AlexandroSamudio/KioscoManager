using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? throw new InvalidOperationException("El ID del usuario no es un entero válido o no se encontró en el token."));
        
        return userId;
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value ?? throw new InvalidOperationException("El email del usuario no se encontró en el token.");
        }
    }
}
