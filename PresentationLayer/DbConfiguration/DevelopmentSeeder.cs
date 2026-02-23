using ApplicationLayer.DTOs.Client;
using ApplicationLayer.DTOs.Projects;
using ApplicationLayer.Interfaces.Services;
using InfrastructureLayer.Context;
using InfrastructureLayer.Identity.Roles;
using InfrastructureLayer.Identity.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

            // Get services
            var organizationService = services.GetRequiredService<IOrganizationService>();
            var clientService = services.GetRequiredService<IClientService>();
            var projectService = services.GetRequiredService<IProjectService>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<Roles>>();

            // === 1. Seed Roles ===
            await SeedRolesAsync(roleManager);

            // === 2. Create Admin User ===
            var adminUser = await CreateAdminUserAsync(userManager);

            // === 3. Create Organization ===
            var organization = await organizationService.CreateOrganizationAsync(
                "Demo Company Ltd", 
                adminUser.Id
            );
            Console.WriteLine($"✓ Organization created: {organization.Name} (ID: {organization.Id})");

            // === 4. Generate API Key ===
            var apiKey = await organizationService.GenerateApiKeyAsync(organization.Id);
            Console.WriteLine($"✓ API Key generated: {apiKey}");
            Console.WriteLine("⚠️  Save this API key - it won't be shown again!");

            // === 5. Create Clients ===
            var client1Id = await clientService.CreateClientAsync(
                organization.Id,
                new ClientCreateDto
                {
                    Name = "Acme Corporation",
                    BillingEmail = "billing@acme.com",
                    PhoneNumber = "+1-555-0101",
                    Address = "123 Main St, New York, NY 10001",
                    Notes = "Large enterprise client"
                },
                adminUser.Id
            );
            Console.WriteLine($"✓ Client created: Acme Corporation (ID: {client1Id})");

            var client2Id = await clientService.CreateClientAsync(
                organization.Id,
                new ClientCreateDto
                {
                    Name = "TechStart Inc",
                    BillingEmail = "accounts@techstart.io",
                    PhoneNumber = "+1-555-0102",
                    Address = "456 Innovation Drive, San Francisco, CA 94105",
                    Notes = "Startup client - monthly retainer"
                },
                adminUser.Id
            );
            Console.WriteLine($"✓ Client created: TechStart Inc (ID: {client2Id})");

            // === 6. Create Projects ===
            var project1Id = await projectService.CreateProjectAsync(
                new ProjectCreateDto
                {
                    
                    Name = "Website Redesign",
                    Description = "Complete overhaul of company website",
                    clientId = client1Id,
                    organizationId = organization.Id
                },
                adminUser.Id
            );
            Console.WriteLine($"✓ Project created: Website Redesign (ID: {project1Id})");

            var project2Id = await projectService.CreateProjectAsync(
                new ProjectCreateDto
                {
                    Name = "Mobile App Development",
                    Description = "iOS and Android native apps",
                    clientId = client1Id,
                    organizationId = organization.Id
                },
                adminUser.Id
            );
            Console.WriteLine($"✓ Project created: Mobile App Development (ID: {project2Id})");

            var project3Id = await projectService.CreateProjectAsync(
                new ProjectCreateDto
                {
                    Name = "Monthly Consulting",
                    Description = "Technical consulting and support",
                    clientId = client2Id,
                    organizationId = organization.Id
                },
                adminUser.Id
            );
            Console.WriteLine($"✓ Project created: Monthly Consulting (ID: {project3Id})");

            // === 7. Create Customer Users ===
            await CreateCustomerUserAsync(userManager, organization.Id, client1Id, "acme.user@acme.com");
            await CreateCustomerUserAsync(userManager, organization.Id, client2Id, "tech.user@techstart.io");

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("✅ Development data seeding completed!");
            Console.WriteLine("========================================");
            Console.WriteLine($"Admin Login: admin@democompany.com / Admin@123");
            Console.WriteLine($"API Key: {apiKey}");
            Console.WriteLine($"Organization ID: {organization.Id}");
            Console.WriteLine("========================================");
        }

        private static async Task SeedRolesAsync(RoleManager<Roles> roleManager)
        {
            string[] roles = ["OrgUser", "Customer"];
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new Roles { Name = role });
                    Console.WriteLine($"✓ Role created: {role}");
                }
            }
        }

        private static async Task<ApplicationUser> CreateAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            var email = "admin@democompany.com";
            var existingUser = await userManager.FindByEmailAsync(email);
            
            if (existingUser != null)
            {
                Console.WriteLine($"✓ Admin user already exists: {email}");
                return existingUser;
            }

            var adminUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User"
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await userManager.AddToRoleAsync(adminUser, "OrgUser");
            Console.WriteLine($"✓ Admin user created: {email} (Password: Admin@123)");
            
            return adminUser;
        }

        private static async Task CreateCustomerUserAsync(
            UserManager<ApplicationUser> userManager,
            Guid organizationId,
            Guid clientId,
            string email)
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            
            if (existingUser != null)
            {
                Console.WriteLine($"✓ Customer user already exists: {email}");
                return;
            }

            var customerUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = "Customer",
                LastName = "User",
                OrganizationId = organizationId,
                ClientId = clientId
            };

            var result = await userManager.CreateAsync(customerUser, "Customer@123");
            if (!result.Succeeded)
            {
                Console.WriteLine($"⚠️  Failed to create customer user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return;
            }

            await userManager.AddToRoleAsync(customerUser, "Customer");
            Console.WriteLine($"✓ Customer user created: {email} (Password: Customer@123)");
        }
    }
}