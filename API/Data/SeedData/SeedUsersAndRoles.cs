using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data.SeedData
{
    public static class SeedUsersAndRoles
    {
        public static async Task SeedRolesAsync(RoleManager<AppRole> roleManager, ILogger logger)
        {
            logger.LogInformation("Attempting to seed roles...");
            string[] roleNames = { "Admin", "Empleado" };

            if (await roleManager.Roles.AnyAsync()) return;

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new AppRole { Name = roleName });
                    if (result.Succeeded)
                    {
                        logger.LogInformation($"Role '{roleName}' created successfully.");
                    }
                    else
                    {
                        logger.LogError($"Error creating role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    logger.LogInformation($"Role '{roleName}' already exists.");
                }
            }
        }

        public static async Task SeedUsersAsync(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, ILogger logger)
        {
            logger.LogInformation("Attempting to seed users...");

            if (await userManager.Users.AnyAsync()) return;

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
                    logger.LogInformation($"Admin user '{adminUserName}' created successfully.");
                    if (await roleManager.RoleExistsAsync("Admin"))
                    {
                        var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                        if (roleResult.Succeeded)
                        {
                            logger.LogInformation($"User '{adminUserName}' added to 'Admin' role.");
                        }
                        else
                        {
                            logger.LogError($"Error adding user '{adminUserName}' to 'Admin' role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        logger.LogWarning("Admin role does not exist. Cannot assign to admin user.");
                    }
                }
                else
                {
                    logger.LogError($"Error creating admin user '{adminUserName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogInformation($"Admin user '{adminUserName}' already exists.");
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
                        logger.LogInformation($"User '{userName}' created successfully.");
                        if (await roleManager.RoleExistsAsync("Employee"))
                        {
                            var roleResult = await userManager.AddToRoleAsync(employeeUser, "Employee");
                            if (roleResult.Succeeded)
                            {
                                logger.LogInformation($"User '{userName}' added to 'Employee' role.");
                            }
                            else
                            {
                                logger.LogError($"Error adding user '{userName}' to 'Employee' role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                            }
                        }
                        else
                        {
                             logger.LogWarning("Employee role does not exist. Cannot assign to user.");
                        }
                    }
                    else
                    {
                        logger.LogError($"Error creating user '{userName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    logger.LogInformation($"User '{userName}' already exists.");
                }
            }
            logger.LogInformation("User seeding finished.");
        }
    }
}
