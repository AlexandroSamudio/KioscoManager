using API.Data;
using API.Entities;
using API.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

namespace API.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddIdentityCore<AppUser>(opt => 
            {
                opt.Password.RequireDigit = true;
                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = true;
                opt.Password.RequireNonAlphanumeric = true;
                opt.Password.RequiredLength = 8;

                opt.User.RequireUniqueEmail = true;
            })
                .AddRoles<AppRole>()
                .AddRoleManager<RoleManager<AppRole>>()
                .AddSignInManager<SignInManager<AppUser>>()            
                .AddEntityFrameworkStores<DataContext>();

            var tokenKey = config["TokenKey"];
            if (string.IsNullOrEmpty(tokenKey))
            {
                throw new InvalidOperationException("La TokenKey no ha sido configurada.");
            }

            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";

                            var response = new
                            {
                                statusCode = 401,
                                message = "Acceso no autorizado.",
                                details = "Debe proporcionar un token de autenticación válido para acceder a este recurso."
                            };

                            return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, jsonSerializerOptions));
                        },
                        OnAuthenticationFailed = context =>
                        {
                            context.NoResult();
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";

                            var response = new
                            {
                                statusCode = 401,
                                message = "Token de autenticación inválido.",
                                details = context.Exception is SecurityTokenExpiredException 
                                    ? "El token ha expirado. Por favor, inicie sesión nuevamente."
                                    : "El token proporcionado no es válido."
                            };

                            return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, jsonSerializerOptions));
                        }
                    };
                });

            services.AddScoped<IAuthorizationHandler, RoleBasedAuthorizationHandler>();

            services.AddAuthorizationBuilder()
                .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new MiembroBlockRequirement())
                    .Build())
                .AddPolicy("RequireAdminRole", policy =>
                    policy.AddRequirements(new AdminOnlyRequirement()));

            return services;
        }
    }
}
