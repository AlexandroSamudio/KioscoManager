using API.Entities;
using API.Controllers;
using API.DTOs;
using API.Interfaces;
using FluentAssertions;
using Moq;
using API.Tests.TestInfrastructure;

namespace API.Tests.Controllers;

public class ConfigControllerTests : ControllerTestBase
{
    private readonly Mock<IConfigRepository> _repo = new();

    private ConfigController CreateController(int kioscoId = 3, int userId = 8)
    {
        var controller = new ConfigController(_repo.Object);
        var principal = TestPrincipalFactory.Create(kioscoId: kioscoId, userId: userId);
        return SetupController(controller, principal);
    }

    [Fact]
    public async Task GetKioscoBasicInfo_Success_Returns_200()
    {
        var dto = new KioscoBasicInfoDto { Id = 3, Nombre = "K" };
        _repo.Setup(r => r.GetKioscoBasicInfoAsync(3, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<KioscoBasicInfoDto>.Success(dto));

        var controller = CreateController(3, 8);
        var res = await controller.GetKioscoBasicInfo(CancellationToken.None);

        res.Result.Should().BeNull();
        res.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task GetKioscoConfig_Success_Returns_200()
    {
        var dto = new KioscoConfigDto { Id = 1, KioscoId = 3 };
        _repo.Setup(r => r.GetKioscoConfigAsync(3, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<KioscoConfigDto>.Success(dto));

        var controller = CreateController(3, 8);
        var res = await controller.GetKioscoConfig(CancellationToken.None);

        res.Result.Should().BeNull();
        res.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task UpdateKioscoConfig_Success_Returns_200()
    {
        var update = new KioscoConfigUpdateDto { Moneda = "USD" };
        var dto = new KioscoConfigDto { Id = 1, KioscoId = 3, Moneda = "USD" };
        _repo.Setup(r => r.UpdateKioscoConfigAsync(3, update, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<KioscoConfigDto>.Success(dto));

        var controller = CreateController(3, 8);
        var res = await controller.UpdateKioscoConfig(update, CancellationToken.None);

        res.Result.Should().BeNull();
        res.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task UpdateKioscoBasicInfo_Success_Returns_200()
    {
        var update = new KioscoBasicInfoUpdateDto { Nombre = "Nuevo" };
        var dto = new KioscoBasicInfoDto { Id = 3, Nombre = "Nuevo" };
        _repo.Setup(r => r.UpdateKioscoBasicInfoAsync(3, update, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<KioscoBasicInfoDto>.Success(dto));

        var controller = CreateController(3, 8);
        var res = await controller.UpdateKioscoBasicInfo(update, CancellationToken.None);

        res.Result.Should().BeNull();
        res.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task GetUserPreferences_Success_Returns_200()
    {
        var dto = new UserPreferencesDto { Id = 1, UserId = 8 };
        _repo.Setup(r => r.GetUserPreferencesAsync(8, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<UserPreferencesDto>.Success(dto));

        var controller = CreateController(3, 8);
        var res = await controller.GetUserPreferences(CancellationToken.None);

        res.Result.Should().BeNull();
        res.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task UpdateUserPreferences_Success_Returns_200()
    {
        var update = new UserPreferencesUpdateDto { NotificacionesVentas = true };
        var dto = new UserPreferencesDto { Id = 2, UserId = 8, NotificacionesVentas = true };
        _repo.Setup(r => r.UpdateUserPreferencesAsync(8, update, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<UserPreferencesDto>.Success(dto));

        var controller = CreateController(3, 8);
        var res = await controller.UpdateUserPreferences(update, CancellationToken.None);

        res.Result.Should().BeNull();
        res.Value.Should().BeEquivalentTo(dto);
    }
}
