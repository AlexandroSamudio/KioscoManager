using API.Controllers;
using API.DTOs;
using API.Interfaces;
using API.Constants;
using API.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using API.Tests.TestInfrastructure;

namespace API.Tests.Controllers;

public class AccountControllerTests : ControllerTestBase
{
    private readonly Mock<IAccountRepository> _repo = new();
    private AccountController CreateController() => SetupController(new AccountController(_repo.Object));

    [Fact]
    public async Task Register_Success_Returns_UserDto()
    {
        var dto = new RegisterDto { Email = "user@example.com", Password = "Passw0rd!", UserName = "john" };
        var expected = new UserDto { Id = 1, Email = dto.Email, UserName = dto.UserName };
        _repo.Setup(r => r.RegisterAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(Result<UserDto>.Success(expected));

        var controller = CreateController();
        var result = await controller.Register(dto, CancellationToken.None);

        result.Result.Should().BeNull();
        result.Value.Should().BeEquivalentTo(expected);
        _repo.Verify(r => r.RegisterAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_Success_Returns_UserDto_With_Token()
    {
        var dto = new LoginDto { Email = "user@example.com", Password = "Passw0rd!" };
        var expected = new UserDto { Id = 2, Email = dto.Email, UserName = "john", Token = "jwt-token" };
        _repo.Setup(r => r.LoginAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(Result<UserDto>.Success(expected));

        var controller = CreateController();
        var result = await controller.Login(dto, CancellationToken.None);

        result.Result.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Token.Should().Be("jwt-token");
        _repo.Verify(r => r.LoginAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_Invalid_Returns_400()
    {
        var dto = new RegisterDto { Email = "bad", Password = "x", UserName = "a" };
        _repo.Setup(r => r.RegisterAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(Result<UserDto>.Failure(ErrorCodes.ValidationError, "invalid"));

        var controller = CreateController();
        var result = await controller.Register(dto, CancellationToken.None);

        var obj = result.Result as ObjectResult;
        obj.Should().NotBeNull();
        obj!.StatusCode.Should().Be(400);
        obj.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns_401()
    {
        var dto = new LoginDto { Email = "user@example.com", Password = "wrong" };
        _repo.Setup(r => r.LoginAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(Result<UserDto>.Failure(ErrorCodes.InvalidCredentials, "wrong creds"));

        var controller = CreateController();
        var result = await controller.Login(dto, CancellationToken.None);

        var obj = result.Result as ObjectResult;
        obj.Should().NotBeNull();
        obj!.StatusCode.Should().Be(401);
        obj.Value.Should().BeOfType<ValidationProblemDetails>();
    }
}
