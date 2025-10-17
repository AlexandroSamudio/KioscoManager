using API.Data.Repositories;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Constants;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace API.Tests.Repositories;

public class AccountRepositoryTests : RepositoryTestBase
{
    private static Mock<UserManager<AppUser>> MockUserManager()
        => new(Mock.Of<IUserStore<AppUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

    [Fact]
    public async Task Login_InvalidEmail_Returns_InvalidCredentials()
    {
        var um = MockUserManager();
        um.Setup(m => m.FindByEmailAsync("no@ex.com")).ReturnsAsync((AppUser?)null);
        var tokenSvc = new Mock<ITokenService>();
        var repo = new AccountRepository(Context, um.Object, tokenSvc.Object, Mapper);

        var result = await repo.LoginAsync(new LoginDto { Email = "no@ex.com", Password = "x" }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.InvalidCredentials);
    }

    [Fact]
    public async Task Login_Success_Returns_Token()
    {
        var user = new AppUser { Id = 1, Email = "u@e.com", UserName = "u" };
        var um = MockUserManager();
        um.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        um.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(true);
        var tokenSvc = new Mock<ITokenService>();
        tokenSvc.Setup(t => t.CreateToken(user)).ReturnsAsync("token");
        var repo = new AccountRepository(Context, um.Object, tokenSvc.Object, Mapper);

        var result = await repo.LoginAsync(new LoginDto { Email = user.Email!, Password = "P@ssw0rd!" }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Token.Should().Be("token");
    }
}
