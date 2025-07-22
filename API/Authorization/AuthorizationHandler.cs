using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API.Authorization
{
    public class MiembroBlockRequirement : IAuthorizationRequirement
    {
    }

    public class AdminOnlyRequirement : IAuthorizationRequirement
    {
    }

    public class RoleBasedAuthorizationHandler : 
        IAuthorizationHandler
    {
        private const string AUTHORIZATION_ERROR_MESSAGE_KEY = "AuthorizationErrorMessage";

        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (var requirement in context.Requirements)
            {
                if (context.HasFailed)
                    break;

                switch (requirement)
                {
                    case MiembroBlockRequirement miembroReq:
                        HandleMiembroBlockRequirement(context, miembroReq);
                        break;
                    case AdminOnlyRequirement adminReq:
                        HandleAdminOnlyRequirement(context, adminReq);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        private void HandleMiembroBlockRequirement(
            AuthorizationHandlerContext context, 
            MiembroBlockRequirement requirement)
        {
            var isMiembro = IsUserInRole(context.User, "miembro");

            if (isMiembro)
            {
                var errorMessage = "Los usuarios con rol 'miembro' no pueden realizar acciones hasta ser asignados a un kiosco por un administrador.";
                StoreErrorMessage(context, errorMessage);
                context.Fail(new AuthorizationFailureReason(this, errorMessage));
                return;
            }

            context.Succeed(requirement);
        }

        private void HandleAdminOnlyRequirement(
            AuthorizationHandlerContext context, 
            AdminOnlyRequirement requirement)
        {
            var isAdmin = IsUserInRole(context.User, "administrador");

            if (!isAdmin)
            {
                var errorMessage = "Esta acción requiere permisos de administrador. Solo los administradores pueden acceder a esta funcionalidad.";
                StoreErrorMessage(context, errorMessage);
                context.Fail(new AuthorizationFailureReason(this, errorMessage));
                return;
            }

            context.Succeed(requirement);
        }

        private static void StoreErrorMessage(AuthorizationHandlerContext context, string message)
        {
            if (context.Resource is HttpContext httpContext)
            {
                httpContext.Items[AUTHORIZATION_ERROR_MESSAGE_KEY] = message;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Could not store authorization error message. Resource type: {context.Resource?.GetType().Name ?? "null"}");
            }
        }

        private static bool IsUserInRole(ClaimsPrincipal user, string role)
        {
            return user.IsInRole(role) || 
                   user.HasClaim("role", role) ||
                   user.HasClaim(ClaimTypes.Role, role);
        }
    }
}
