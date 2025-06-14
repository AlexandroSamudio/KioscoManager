using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new InvalidOperationException("El ID del usuario no se encontró en el token o no es válido.");
            }
            
            return userId;
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value ?? throw new InvalidOperationException("El email del usuario no se encontró en el token.");
        }

        public static int GetKioscoId(this ClaimsPrincipal user)
        {
            var kioscoIdClaim = user.FindFirst("kioscoId")?.Value;
            if (string.IsNullOrEmpty(kioscoIdClaim) || !int.TryParse(kioscoIdClaim, out var kioscoId))
            {
                throw new InvalidOperationException("El KioscoId del usuario no se encontró en el token o no es válido.");
            }
            return kioscoId;
        }
    }
}
