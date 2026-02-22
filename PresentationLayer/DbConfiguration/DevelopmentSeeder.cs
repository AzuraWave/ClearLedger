using InfrastructureLayer.Context;
using DomainLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using InfrastructureLayer.Identity.User;
using InfrastructureLayer.Identity.Roles;

namespace PresentationLayer.DbConfiguration
{
    public static class DevelopmentSeeder
    {
        public static async Task SeedDevelopmentDataAsync(LedgerDbContext context, IServiceProvider services)
        {
            // Check if already seeded
            if (await context.Organizations.AnyAsync())
            {
                Console.WriteLine("Database already contains data. Skipping seeding.");
                return;
            }

            Console.WriteLine("Seeding development data...");

            // Sample organization with API key
            var orgId = Guid.NewGuid();
            var apiKey = "dev-api-key-12345";
            
            var organization = new Organization
            {
                Id = orgId,
                Name = "Demo Company Ltd",
                ApiKeyHash = HashApiKey(apiKey),
                DefaultAutomationUserId = Guid.NewGuid(),
                CreatedBy = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            context.Organizations.Add(organization);

            // Create sample clients
            var client1 = new Client
            {
                Id = Guid.NewGuid(),
                Name = "Acme Corporation",
                BillingEmail = "billing@acme.com",
                Address = "123 Main St, New York, NY 10001",
                OrganizationId = orgId,
                CreatedBy = organization.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            var client2 = new Client
            {
                Id = Guid.NewGuid(),
                Name = "Tech Startup Inc",
                BillingEmail = "finance@techstartup.com",
                Address = "456 Silicon Valley, CA 94025",
                OrganizationId = orgId,
                CreatedBy = organization.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            context.Clients.AddRange(client1, client2);

            // Create sample projects
            var project1 = new Project
            {
                Id = Guid.NewGuid(),
                Name = "Website Redesign",
                Description = "Complete website overhaul",
                ClientId = client1.Id,
                OrganizationId = orgId,
                Status = DomainLayer.Enums.ProjectStatus.Active,
                CreatedBy = organization.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            var project2 = new Project
            {
                Id = Guid.NewGuid(),
                Name = "Mobile App Development",
                Description = "iOS and Android app",
                ClientId = client2.Id,
                OrganizationId = orgId,
                Status = DomainLayer.Enums.ProjectStatus.Active,
                CreatedBy = organization.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            context.Projects.AddRange(project1, project2);  

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<Roles>>();

            // Create roles first
            await DbInitializer.SeedRolesAsync(services);

            // Create sample OrgUser
            var orgUser = new ApplicationUser
            {
                UserName = "demo@clearledger.com",
                Email = "demo@clearledger.com",
                EmailConfirmed = true,
                OrganizationId = orgId
            };

            var result = await userManager.CreateAsync(orgUser, "Demo123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(orgUser, "OrgUser");
            }

            // Create customer user for Acme Corporation
            var customer1 = new ApplicationUser
            {
                UserName = "john.doe@acme.com",
                Email = "john.doe@acme.com",
                EmailConfirmed = true,
                OrganizationId = orgId,
                ClientId = client1.Id
            };

            result = await userManager.CreateAsync(customer1, "Customer123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(customer1, "Customer");
            }

            // Create customer user for Tech Startup Inc
            var customer2 = new ApplicationUser
            {
                UserName = "jane.smith@techstartup.com",
                Email = "jane.smith@techstartup.com",
                EmailConfirmed = true,
                OrganizationId = orgId,
                ClientId = client2.Id
            };

            result = await userManager.CreateAsync(customer2, "Customer123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(customer2, "Customer");
            }




            await context.SaveChangesAsync();

            Console.WriteLine("✅ Development data seeded successfully!");
            Console.WriteLine($"📝 Organization: {organization.Name}");
            Console.WriteLine($"🔑 API Key: {apiKey}");
            Console.WriteLine($"👥 Clients: {client1.Name}, {client2.Name}");
            Console.WriteLine($"📊 Projects: {project1.Name}, {project2.Name}");
            Console.WriteLine($"👤 OrgUser: demo@clearledger.com / Demo123!");
            Console.WriteLine($"👤 Customer 1: john.doe@acme.com / Customer123! (Acme Corporation)");
            Console.WriteLine($"👤 Customer 2: jane.smith@techstartup.com / Customer123! (Tech Startup Inc)");
        }

        private static string HashApiKey(string apiKey)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(apiKey));
            return Convert.ToBase64String(bytes);
        }
    }
}
