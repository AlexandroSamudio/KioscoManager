using API.Data;
using API.Helpers;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace API.Tests.Repositories;

public abstract class RepositoryTestBase : IDisposable
{
    protected readonly DataContext Context;
    protected readonly IMapper Mapper;

    protected RepositoryTestBase()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        Context = new DataContext(options);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AutoMapperProfiles>();
        });
        Mapper = config.CreateMapper();
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
        (Mapper as IDisposable)?.Dispose();
    }
}
