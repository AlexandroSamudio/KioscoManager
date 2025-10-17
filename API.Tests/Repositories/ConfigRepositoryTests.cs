using API.Data.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace API.Tests.Repositories;

public class ConfigRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task GetKioscoConfig_Creates_Default_When_Missing()
    {
        var repo = new ConfigRepository(Context, Mapper);

        var result = await repo.GetKioscoConfigAsync(7, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.KioscoId.Should().Be(7);
        (await Context.KioscoConfigs.CountAsync()).Should().Be(1);
    }
}
