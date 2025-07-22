using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API.Authorization
{
    public static class ApplicationRoles
    {
        public const string Miembro = "miembro";
        public const string Administrador = "administrador";
    }
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
            var isMiembro = IsUserInRole(context.User, ApplicationRoles.Miembro);

            if (isMiembro)
            {
                FailWithErrorMessage(context, requirement, 
                    "Los usuarios con rol 'miembro' no pueden realizar acciones hasta ser asignados a un kiosco por un administrador.");
                return;
            }

            context.Succeed(requirement);
        }

        private void HandleAdminOnlyRequirement(
            AuthorizationHandlerContext context, 
            AdminOnlyRequirement requirement)
        {
            var isAdmin = IsUserInRole(context.User, ApplicationRoles.Administrador);

            if (!isAdmin)
            {
                FailWithErrorMessage(context, requirement,
                    "Esta acción requiere permisos de administrador. Solo los administradores pueden acceder a esta funcionalidad.");
                return;
            }

            context.Succeed(requirement);
        }

        private void FailWithErrorMessage(
            AuthorizationHandlerContext context, 
            IAuthorizationRequirement requirement, 
            string errorMessage)
        {
            StoreErrorMessage(context, errorMessage);
            context.Fail(new AuthorizationFailureReason(this, errorMessage));
        }

        private static void StoreErrorMessage(AuthorizationHandlerContext context, string message)
        {
            if (context.Resource is HttpContext httpContext)
            {
                httpContext.Items[AUTHORIZATION_ERROR_MESSAGE_KEY] = message;
            }
        }

        private static bool IsUserInRole(ClaimsPrincipal user, string role)
        {
            if (user.IsInRole(role))
                return true;

            return user.Claims.Any(c => 
                       (c.Type == "role" || c.Type == ClaimTypes.Role) && 
                       string.Equals(c.Value, role, StringComparison.OrdinalIgnoreCase));
        }
    }
}
