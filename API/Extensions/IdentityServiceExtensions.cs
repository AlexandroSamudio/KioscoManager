using API.Data;
using API.Entities;
using API.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions
{
    public static class IdentityServiceExtensions
    {
        private static readonly string[] TokenErrorMessages = ["Debe proporcionar un token de autenticación válido para acceder a este recurso."];

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
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                context.Response.ContentType = "application/problem+json";
                                context.Response.Headers.WWWAuthenticate = "Bearer realm=\"api\", error=\"invalid_token\", error_description=\"Token ausente o invalido\"";

                                var problemDetails = new ProblemDetails
                                {
                                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                                    Title = "Acceso no autorizado.",
                                    Status = StatusCodes.Status401Unauthorized,
                                    Detail = "Debe proporcionar un token de autenticación válido para acceder a este recurso.",
                                    Instance = context.Request?.Path
                                };
                                problemDetails.Extensions["errors"] = new Dictionary<string, string[]>
                                {
                                    { "token", TokenErrorMessages }
                                };

                                return context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonSerializerOptions));
                            },
                            OnAuthenticationFailed = context =>
                            {
                                context.NoResult();
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                context.Response.ContentType = "application/problem+json";
                                if (context.Exception is SecurityTokenExpiredException stex)
                                {
                                    context.Response.Headers.WWWAuthenticate =
                                      $"Bearer realm=\"api\", error=\"invalid_token\", error_description=\"El token expiró en {stex.Expires:O}\"";
                                }
                                else
                                {
                                    context.Response.Headers.WWWAuthenticate =
                                      "Bearer realm=\"api\", error=\"invalid_token\", error_description=\"Token invalido\"";
                                }

                                var problemDetails = new ProblemDetails
                                {
                                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                                    Title = "Token de autenticación invalido.",
                                    Status = StatusCodes.Status401Unauthorized,
                                    Detail = context.Exception is SecurityTokenExpiredException
                                        ? "El token ha expirado. Por favor, inicie sesión nuevamente."
                                        : "El token proporcionado no es valido.",
                                    Instance = context.Request?.Path
                                };

                                return context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonSerializerOptions));
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
                    policy.AddRequirements(new AdminOnlyRequirement()))
                .AddPolicy("AllowMiembroForKioscoCreation", policy =>
                    policy.RequireAuthenticatedUser());

            return services;
        }
    }
}
