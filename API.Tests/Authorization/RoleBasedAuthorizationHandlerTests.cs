using System.Security.Claims;
using API.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace API.Tests.Authorization;

public class RoleBasedAuthorizationHandlerTests
{
    private const string ErrorKey = "AuthorizationErrorMessage";
    private static RoleBasedAuthorizationHandler Handler() => new();

    [Fact]
    public async Task MiembroBlockRequirement_Should_Fail_For_Miembro_And_Store_Message()
    {
        var requirement = new MiembroBlockRequirement();
        var httpContext = new DefaultHttpContext();
        var user = PrincipalWithRoles((ClaimTypes.Role, ApplicationRoles.Miembro));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, httpContext);

        await Handler().HandleAsync(context);

        context.HasFailed.Should().BeTrue();
        httpContext.Items.ContainsKey(ErrorKey).Should().BeTrue();
        httpContext.Items[ErrorKey].Should().BeOfType<string>()
            .Which.Should().Be("Los usuarios con rol 'miembro' no pueden realizar acciones hasta ser asignados a un kiosco por un administrador.");
    }

    [Fact]
    public async Task MiembroBlockRequirement_Should_Succeed_When_Not_Miembro()
    {
        var requirement = new MiembroBlockRequirement();
        var httpContext = new DefaultHttpContext();
        var user = PrincipalWithRoles((ClaimTypes.Role, "invitado"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, httpContext);

        await Handler().HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task AdminOnlyRequirement_Should_Fail_For_Non_Admin()
    {
        var requirement = new AdminOnlyRequirement();
        var httpContext = new DefaultHttpContext();
        var user = PrincipalWithRoles((ClaimTypes.Role, "miembro"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, httpContext);

        await Handler().HandleAsync(context);

        context.HasFailed.Should().BeTrue();
        httpContext.Items.ContainsKey(ErrorKey).Should().BeTrue();
        httpContext.Items[ErrorKey].Should().BeOfType<string>()
            .Which.Should().Be("Esta acción requiere permisos de administrador. Solo los administradores pueden acceder a esta funcionalidad.");
    }

    [Fact]
    public async Task AdminOnlyRequirement_Should_Succeed_For_Admin()
    {
        var requirement = new AdminOnlyRequirement();
        var httpContext = new DefaultHttpContext();
        var user = PrincipalWithRoles((ClaimTypes.Role, ApplicationRoles.Administrador));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, httpContext);

        await Handler().HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Handle_Role_And_ClaimTypesRole_With_Case_Insensitive_Comparison()
    {
        var adminRequirement = new AdminOnlyRequirement();
        var httpContext1 = new DefaultHttpContext();
        var userWithCustomRoleClaim = PrincipalWithRoles(("role", "ADMINISTRADOR"));
        var ctx1 = new AuthorizationHandlerContext(new[] { adminRequirement }, userWithCustomRoleClaim, httpContext1);
        await Handler().HandleAsync(ctx1);
        ctx1.HasSucceeded.Should().BeTrue();

        var miembroRequirement = new MiembroBlockRequirement();
        var httpContext2 = new DefaultHttpContext();
        var userWithClaimTypesRole = PrincipalWithRoles((ClaimTypes.Role, "MiEmBrO"));
        var ctx2 = new AuthorizationHandlerContext(new[] { miembroRequirement }, userWithClaimTypesRole, httpContext2);
        await Handler().HandleAsync(ctx2);
        ctx2.HasFailed.Should().BeTrue();
        httpContext2.Items.ContainsKey(ErrorKey).Should().BeTrue();
    }

    private static ClaimsPrincipal PrincipalWithRoles(params (string type, string value)[] roleClaims)
    {
        var claims = roleClaims.Select(rc => new Claim(rc.type, rc.value));
        var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
        return new ClaimsPrincipal(identity);
    }
}
