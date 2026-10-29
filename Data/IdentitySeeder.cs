using Microsoft.AspNetCore.Identity;
using RealEstateApi.Models;
using RealEstateApi.Services.Common;

namespace RealEstateApi.Data;

/// <summary>
/// Ініціалізатор системних ролей та облікового запису адміністратора під час старту застосунку.
/// </summary>
public static class IdentitySeeder
{
    /// <summary>
    /// Виконує перевірку наявності базових ролей та адміністратора, створюючи їх за необхідності.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");

        foreach (var role in new[] { AppRoles.Administrator, AppRoles.User })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var email = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var admin = await userManager.FindByEmailAsync(email);

        if (admin == null)
        {
            admin = new ApplicationUser 
            { 
                UserName = email, 
                Email = email, 
                EmailConfirmed = true 
            };
            
            var created = await userManager.CreateAsync(admin, password);

            if (!created.Succeeded)
            {
                logger.LogWarning("Не вдалося створити адміністратора за замовчуванням: {Errors}",
                    string.Join("; ", created.Errors.Select(e => e.Description)));
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(admin, AppRoles.Administrator))
            await userManager.AddToRoleAsync(admin, AppRoles.Administrator);
    }
}