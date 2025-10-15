using System;
using System.Linq;
using System.Threading.Tasks;
using EduResolve.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EduResolve.Data
{
    public static class SeedData
    {
        private static readonly string[] DefaultRoles = { "Admin", "HOD", "Teacher", "Student" };

        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var context = scopedServices.GetRequiredService<ApplicationDbContext>();
            var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();

            await context.Database.MigrateAsync();

            await EnsureRolesAsync(roleManager);
            await EnsureDepartmentsAsync(context);
            await EnsureAdminAsync(userManager);
            await EnsureHodsAsync(userManager, context);
        }

        private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in DefaultRoles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task EnsureDepartmentsAsync(ApplicationDbContext context)
        {
            if (await context.Departments.AnyAsync())
            {
                return;
            }

            var departments = new[]
            {
                new Department { Name = "Computer Engineering" },
                new Department { Name = "Electrical Engineering" },
                new Department { Name = "Mechanical Engineering" },
                new Department { Name = "Library" }
            };

            context.Departments.AddRange(departments);
            await context.SaveChangesAsync();
        }

        private static async Task EnsureAdminAsync(UserManager<ApplicationUser> userManager)
        {
            const string adminEmail = "admin@eduresolve.local";
            if (await userManager.FindByEmailAsync(adminEmail) != null)
            {
                return;
            }

            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin"
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                return;
            }

            throw new InvalidOperationException($"Failed to create default admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        private static async Task EnsureHodsAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            var departments = await context.Departments.ToListAsync();

            foreach (var department in departments)
            {
                if (department == null)
                {
                    continue;
                }

                var email = $"{GenerateSlug(department.Name)}.hod@eduresolve.local";

                if (await userManager.FindByEmailAsync(email) != null)
                {
                    continue;
                }

                var hodUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = department.Name,
                    LastName = "HOD",
                    DepartmentId = department.Id
                };

                var result = await userManager.CreateAsync(hodUser, "Hod@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(hodUser, "HOD");
                }
                else
                {
                    throw new InvalidOperationException($"Failed to create HOD user for department '{department.Name}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        private static string GenerateSlug(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "department";
            }

            var characters = input
                .Trim()
                .ToLowerInvariant()
                .Select(c => char.IsLetterOrDigit(c) ? c : '-')
                .ToArray();

            var slug = new string(characters);
            while (slug.Contains("--"))
            {
                slug = slug.Replace("--", "-");
            }

            return slug.Trim('-');
        }
    }
}
