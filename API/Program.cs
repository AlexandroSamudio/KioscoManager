using API.Extensions;
using API.Data.SeedData;
using API.Middleware;
using API.Helpers;
using API.Validators;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using FluentValidation.AspNetCore;
using FluentValidation;
using API.Interfaces;
using API.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
  .AddJsonOptions(o =>
  {
      o.JsonSerializerOptions.DefaultIgnoreCondition =
        System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
      o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
      o.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
  });
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CompraCreateDtoValidator>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = CustomValidationResponseFactory.CreateValidationProblemResponse;
});
builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "KioscoManager API",
        Description = "API para la gestión de kioscos",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autorizacion JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
            }
        },
        Array.Empty<string>()
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });

    options.DocInclusionPredicate((name, api) => true);

});


builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;

    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();

    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/json",
        "application/xml",
        "text/json",
        "image/svg+xml"
    });
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        var origins = builder.Environment.IsDevelopment() 
           ? new[] { "http://localhost:4200", "https://localhost:4200" }
           : ["https://kioscomanager.com", "https://www.kioscomanager.com"];

        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "es-AR" };
    options.SetDefaultCulture("es-AR");
});

builder.Services.AddOpenApi();

builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "KioscoManager API v1");
        options.RoutePrefix = "api-docs";
        options.DocumentTitle = "KioscoManager API Documentation";

        options.DefaultModelsExpandDepth(2);
        options.DefaultModelExpandDepth(2);
        options.DisplayRequestDuration();
    });
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseResponseCompression();

app.UseCors("AllowAngularApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DBSeeder.SeedAllAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al sembrar la base de datos");
    }
}


app.Run();