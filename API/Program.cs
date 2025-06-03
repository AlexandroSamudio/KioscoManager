using API.Extensions;
using API.Data.SeedData;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddCors();

builder.Services.AddOpenApi();


builder.Services.AddIdentityServices(builder.Configuration);


var app = builder.Build();


app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("http://localhost:4200","https://localhost:4200"));
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DBSeeder.SeedAllAsync(services);
}


app.Run();