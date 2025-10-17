using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Entities;
using API.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace API.Tests.Services;

public class TokenServiceTests
{
    private static IConfiguration BuildConfig(string? tokenKey)
    {
        var inMemory = new Dictionary<string, string?>
        {
            ["TokenKey"] = tokenKey
        }!;
        return new ConfigurationBuilder().AddInMemoryCollection(inMemory!).Build();
    }

    private static AppUser DummyUser() => new() { Id = 123, Email = "test@example.com", KioscoId = 42 };

    private static UserManager<AppUser> MockUserManager(IEnumerable<string> roles)
    {
        var store = new Mock<IUserStore<AppUser>>();
        var mgr = new Mock<UserManager<AppUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        mgr.Setup(x => x.GetRolesAsync(It.IsAny<AppUser>())).ReturnsAsync(roles.ToList());
        return mgr.Object;
    }

    [Fact]
    public async Task CreateToken_Throws_When_TokenKey_Missing()
    {
        var config = BuildConfig(null);
        var userManager = MockUserManager(Array.Empty<string>());
        var timeProvider = TimeProvider.System;
        var svc = new TokenService(config, userManager, timeProvider);

        Func<Task> act = async () => await svc.CreateToken(DummyUser());
        await act.Should().ThrowAsync<Exception>().WithMessage("No se ha configurado la TokenKey");
    }

    [Theory]
    [InlineData(10)]
    [InlineData(63)]
    public async Task CreateToken_Throws_When_TokenKey_TooShort(int length)
    {
        var key = new string('a', length);
        var config = BuildConfig(key);
        var userManager = MockUserManager(Array.Empty<string>());
        var timeProvider = TimeProvider.System;
        var svc = new TokenService(config, userManager, timeProvider);

        Func<Task> act = async () => await svc.CreateToken(DummyUser());
        await act.Should().ThrowAsync<Exception>().WithMessage("Tu TokenKey debe tener al menos 64 caracteres");
    }

    [Fact]
    public async Task CreateToken_Includes_Claims_UserId_Email_KioscoId_Roles()
    {
        var key = new string('b', 64);
        var config = BuildConfig(key);
        var roles = new[] { "Admin", "Seller" };
        var userManager = MockUserManager(roles);

        var frozenUtc = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var fakeTime = new FakeTimeProvider(frozenUtc);

        var svc = new TokenService(config, userManager, fakeTime);
        var user = DummyUser();

        var token = await svc.CreateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.NameId && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        jwt.Claims.Should().Contain(c => c.Type == "kioscoId" && c.Value == user.KioscoId!.ToString());
        roles.All(r => jwt.Claims.Any(c => (c.Type == ClaimTypes.Role || c.Type == "role") && c.Value == r)).Should().BeTrue();
    }

    [Fact]
    public async Task CreateToken_Sets_Expiry_About_7_Days_From_Now_Using_TimeProvider()
    {
        var key = new string('c', 64);
        var config = BuildConfig(key);
        var userManager = MockUserManager(Array.Empty<string>());

        var frozenUtc = new DateTimeOffset(2024, 5, 10, 8, 30, 0, TimeSpan.Zero);
        var fakeTime = new FakeTimeProvider(frozenUtc);

        var svc = new TokenService(config, userManager, fakeTime);
        var token = await svc.CreateToken(DummyUser());

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.ValidTo.Should().BeCloseTo(frozenUtc.UtcDateTime.AddDays(7), TimeSpan.FromSeconds(5));
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;
        public FakeTimeProvider(DateTimeOffset now) => _now = now;
        public override DateTimeOffset GetUtcNow() => _now;
    }
}
