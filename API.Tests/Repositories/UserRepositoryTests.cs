using API.Data.Repositories;
using API.DTOs;
using API.Entities;
using API.Constants;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace API.Tests.Repositories;

public class UserRepositoryTests : RepositoryTestBase
{
    private static Mock<UserManager<AppUser>> MockUserManager()
        => new(Mock.Of<IUserStore<AppUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

    private static Mock<RoleManager<AppRole>> MockRoleManager()
        => new(new Mock<IRoleStore<AppRole>>().Object, null!, null!, null!, null!);

    [Fact]
    public async Task GetUserById_Returns_User_With_Role()
    {
        var user = new AppUser { UserName = "john", Email = "j@e.com" };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        var um = MockUserManager();
        um.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["administrador"]);
        var rm = MockRoleManager();
        var repo = new UserRepository(Context, um.Object, rm.Object, Mapper);

        var result = await repo.GetUserByIdAsync(user.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Role.Should().Be("administrador");
    }

    [Fact]
    public async Task UpdateProfile_DuplicateEmail_Returns_FieldExists()
    {
        Context.Users.AddRange(new AppUser { Id = 1, Email = "a@e.com", UserName = "a" }, new AppUser { Id = 2, Email = "b@e.com", UserName = "b" });
        await Context.SaveChangesAsync();

        var um = MockUserManager();
        var rm = MockRoleManager();
        var repo = new UserRepository(Context, um.Object, rm.Object, Mapper);

        var result = await repo.UpdateProfileAsync(1, new ProfileUpdateDto { Email = "b@e.com", UserName = "a" }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.FieldExists);
    }
}
