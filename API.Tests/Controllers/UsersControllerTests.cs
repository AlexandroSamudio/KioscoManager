using API.Entities;
using API.Controllers;
using API.DTOs;
using API.Interfaces;
using API.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using API.Tests.TestInfrastructure;

namespace API.Tests.Controllers;

public class UsersControllerTests : ControllerTestBase
{
    private readonly Mock<IUserRepository> _repo = new();

    private UsersController CreateController(int kioscoId = 2, int userId = 10)
    {
        var controller = new UsersController(_repo.Object);
        var principal = TestPrincipalFactory.Create(kioscoId: kioscoId, userId: userId);
        return SetupController(controller, principal);
    }

    [Fact]
    public async Task GetUser_Success_Returns_200()
    {
        var dto = new UserManagementDto { Id = 5, Email = "e@x.com" };
        _repo.Setup(r => r.GetUserByIdAsync(5, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<UserManagementDto>.Success(dto));

        var controller = CreateController();
        var res = await controller.GetUser(5, CancellationToken.None);

        res.Result.Should().BeNull();
        res.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task GetUsersByKiosco_Success_Returns_Paged_Ok_With_Header()
    {
        var paged = new PagedList<UserManagementDto>(new[] { new UserManagementDto { Id = 1 } }, 1, 1, 10);
        _repo.Setup(r => r.GetUsersByKioscoAsync(2, 1, 10, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<PagedList<UserManagementDto>>.Success(paged));

        var controller = CreateController(2, 10);
        var res = await controller.GetUsersByKiosco(2, CancellationToken.None, 1, 10);

        var ok = res.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().BeEquivalentTo(paged);
        controller.Response.Headers.ContainsKey("Pagination").Should().BeTrue();
    }

    [Fact]
    public async Task AssignRole_Success_Returns_200()
    {
        var resultDto = new UserRoleResponseDto { UserId = 5, Role = "Admin" };
        _repo.Setup(r => r.AssignRoleAsync(5, "Admin", 10, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<UserRoleResponseDto>.Success(resultDto));

        var controller = CreateController(2, 10);
        var res = await controller.AssignRole(5, new UserRoleAssignmentDto { Role = "Admin" }, CancellationToken.None);

        res.Result.Should().BeNull();
        res.Value.Should().BeEquivalentTo(resultDto);
    }

    [Fact]
    public async Task IsUserAdmin_Success_Returns_200()
    {
        _repo.Setup(r => r.IsUserAdminAsync(5, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<bool>.Success(true));

        var controller = CreateController();
        var res = await controller.IsUserAdmin(5, CancellationToken.None);

        res.Result.Should().BeNull();
        res.Value.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProfile_WrongUser_Returns_403()
    {
        var controller = CreateController(userId: 11);
        var res = await controller.UpdateProfile(10, new ProfileUpdateDto { UserName = "new" }, CancellationToken.None);

        var obj = res as ObjectResult;
        obj!.StatusCode.Should().Be(403);
        obj.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public async Task UpdateProfile_Success_Returns_204()
    {
        _repo.Setup(r => r.UpdateProfileAsync(10, It.IsAny<ProfileUpdateDto>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result.Success());

        var controller = CreateController(2, 10);
        var res = await controller.UpdateProfile(10, new ProfileUpdateDto { UserName = "new" }, CancellationToken.None);

        res.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ChangePassword_WrongUser_Returns_403()
    {
        var controller = CreateController(userId: 11);
        var res = await controller.ChangePassword(10, new ChangePasswordDto { CurrentPassword = "a", NewPassword = "b" }, CancellationToken.None);

        var obj = res as ObjectResult;
        obj!.StatusCode.Should().Be(403);
        obj.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public async Task ChangePassword_Success_Returns_204()
    {
        _repo.Setup(r => r.ChangePasswordAsync(10, It.IsAny<ChangePasswordDto>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result.Success());

        var controller = CreateController(2, 10);
        var res = await controller.ChangePassword(10, new ChangePasswordDto { CurrentPassword = "a", NewPassword = "b" }, CancellationToken.None);

        res.Should().BeOfType<NoContentResult>();
    }
}
