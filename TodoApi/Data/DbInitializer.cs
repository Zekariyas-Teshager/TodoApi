using Microsoft.AspNetCore.Identity;
using TodoApi.Data;
using TodoApi.Models;

public static class DbInitializer
{
    public static async Task Initialize(
        TodoContext context,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager
    )
    {
        // Ensure database is created
        context.Database.EnsureCreated();

        // Seed roles if not exist
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }
        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
        }

        // Create admin user if not exists
        var adminEmail = "admin@todoapi.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                Console.WriteLine("Admin user created successfully.");
                var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (roleResult.Succeeded)
                {
                    Console.WriteLine("Admin user assigned to Admin role successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to assign Admin role to admin user:");
                    foreach (var error in roleResult.Errors)
                    {
                        Console.WriteLine($"- {error.Description}");
                    }
                }
            }
        }

        // Check if data already exists
        if (context.TodoItems.Any())
        {
            return; // DB has been seeded already
        }

        // Create seed data
        var todoItems = new TodoItem[]
        {
            new TodoItem
            {
                Name = "Complete project proposal",
                IsComplete = false,
                Secret = "Due by Friday",
            },
            new TodoItem
            {
                Name = "Review pull requests",
                IsComplete = true,
                Secret = "Team is waiting",
            },
            new TodoItem
            {
                Name = "Update documentation",
                IsComplete = false,
                Secret = "New features added",
            },
        };

        // Add to context and save
        context.TodoItems.AddRange(todoItems);
        context.SaveChanges();
    }
}
