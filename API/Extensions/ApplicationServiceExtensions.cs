using API.Data;
using API.Data.Repositories;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<DataContext>(opt =>
        {
            opt.UseNpgsql(config.GetConnectionString("DefaultConnection"));

        });

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        
        return services;

    }

}
