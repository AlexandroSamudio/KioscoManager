using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data.SeedData
{
    public static class SeedUsersAndRoles
    {
        public static async Task SeedRolesAsync(RoleManager<AppRole> roleManager, ILogger logger)
        {
            logger.LogInformation("Intentando sembrar roles...");
            string[] roleNames = { "administrador", "empleado","miembro" };

            if (await roleManager.Roles.AnyAsync())
            {
                logger.LogInformation("Los roles ya existen en la base de datos. No se requiere sembrado de roles.");
                return;
            }

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new AppRole { Name = roleName });
                    if (result.Succeeded)
                    {
                        logger.LogInformation($"Rol '{roleName}' creado exitosamente.");
                    }
                    else
                    {
                        logger.LogError($"Error creando el rol '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    logger.LogInformation($"Rol '{roleName}' ya existe.");
                }
            }
            logger.LogInformation("Sembrado de roles finalizado.");
        }

        public static async Task SeedUsersAsync(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, ILogger logger)
        {
            logger.LogInformation("Intentando sembrar usuarios...");

            if (await userManager.Users.AnyAsync())
            {
                logger.LogInformation("Los usuarios ya existen en la base de datos. No se requiere sembrado de usuarios.");
                return;
            }

            var adminEmail = "admin@example.com";
            var adminUserName = "admin";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new AppUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "AdminPa$$w0rd1");

                if (result.Succeeded)
                {
                    logger.LogInformation($"Usuario administrador '{adminUserName}' creado exitosamente.");
                    if (await roleManager.RoleExistsAsync("administrador")) 
                    {
                        var roleResult = await userManager.AddToRoleAsync(adminUser, "administrador");
                        if (roleResult.Succeeded)
                        {
                            logger.LogInformation($"Usuario '{adminUserName}' añadido al rol 'administrador'.");
                        }
                        else
                        {
                            logger.LogError($"Error añadiendo usuario '{adminUserName}' al rol 'administrador': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        logger.LogWarning("El rol 'administrador' no existe. No se puede asignar al usuario administrador.");
                    }
                }
                else
                {
                    logger.LogError($"Error creando usuario administrador '{adminUserName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogInformation($"Usuario administrador '{adminUserName}' ya existe.");
            }

            var employeeUsers = new List<(string UserName, string Email, string Password)>
            {
                ("employee1", "employee1@example.com", "EmpPa$$w0rd1"),
                ("employee2", "employee2@example.com", "EmpPa$$w0rd2")
            };

            foreach (var (userName, email, password) in employeeUsers)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var employeeUser = new AppUser
                    {
                        UserName = userName,
                        Email = email,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(employeeUser, password);

                    if (result.Succeeded)
                    {
                        logger.LogInformation($"Usuario '{userName}' creado exitosamente.");
                        if (await roleManager.RoleExistsAsync("empleado")) 
                        {
                            var roleResult = await userManager.AddToRoleAsync(employeeUser, "empleado");
                            if (roleResult.Succeeded)
                            {
                                logger.LogInformation($"Usuario '{userName}' añadido al rol 'empleado'.");
                            }
                            else
                            {
                                logger.LogError($"Error añadiendo usuario '{userName}' al rol 'empleado': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                            }
                        }
                        else
                        {
                             logger.LogWarning("El rol 'empleado' no existe. No se puede asignar al usuario.");
                        }
                    }
                    else
                    {
                        logger.LogError($"Error creando usuario '{userName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    logger.LogInformation($"Usuario '{userName}' ya existe.");
                }
            }
            logger.LogInformation("Sembrado de usuarios finalizado.");
        }
    }
}
