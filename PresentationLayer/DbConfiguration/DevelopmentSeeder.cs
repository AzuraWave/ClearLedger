using ApplicationLayer.DTOs.Client;
using ApplicationLayer.DTOs.Projects;
using ApplicationLayer.DTOs.Transactions.Adjustment;
using ApplicationLayer.DTOs.Transactions.Discount;
using ApplicationLayer.DTOs.Transactions.Invoices;
using ApplicationLayer.DTOs.Transactions.Payments;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Enums;
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
            var transactionService = services.GetRequiredService<ITransactionService>();
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

            // === 3.5. Assign Organization to Admin User ===
            adminUser.OrganizationId = organization.Id;
            var updateResult = await userManager.UpdateAsync(adminUser);
            if (!updateResult.Succeeded)
            {
                throw new Exception($"Failed to assign organization to admin user: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
            }
            Console.WriteLine($"✓ Admin user assigned to organization: {organization.Name}");

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

            // === 8. Create Transactions ===
            await SeedTransactionsAsync(transactionService, organization.Id, client1Id, client2Id, 
                project1Id, project2Id, project3Id, adminUser.Id);

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

        private static async Task SeedTransactionsAsync(
            ITransactionService transactionService,
            Guid organizationId,
            Guid client1Id,
            Guid client2Id,
            Guid project1Id,
            Guid project2Id,
            Guid project3Id,
            Guid createdBy)
        {
            Console.WriteLine();
            Console.WriteLine("Seeding transactions...");

            // === Invoice 1: Website Redesign - Initial Phase ===
            var invoice1Id = await transactionService.CreateInvoiceAsync(new CreateInvoiceDto
            {
                ClientId = client1Id,
                ProjectId = project1Id,
                Amount = 15000.00m,
                Reference = "Website Redesign - Phase 1: Discovery & Design",
                Date = DateTime.UtcNow.AddDays(-45),
                Lines = new List<CreateInvoiceLineDto>
                {
                    new CreateInvoiceLineDto{
                        Description = "Discovery and requirements gathering",
                        Quantity = 1,
                        UnitPrice = 5000.00m
                    },
                    new CreateInvoiceLineDto{
                        Description = "Wireframing and prototyping",
                        Quantity = 1,
                        UnitPrice = 5000.00m
                    },
                    new CreateInvoiceLineDto{
                        Description = "Visual design and mockups",
                        Quantity = 1,
                        UnitPrice = 5000.00m
                    }
                }
            }, organizationId, createdBy);
            Console.WriteLine($"✓ Invoice created: Website Redesign Phase 1 ($15,000.00)");

            // Payment for Invoice 1
            await transactionService.CreateProjectPaymentAsync(new CreateProjectPaymentDto
            {
                ProjectId = project1Id,
                Amount = 15000.00m,
                Reference = "Payment for Website Redesign Phase 1",
                Date = DateTime.UtcNow.AddDays(-30),
            }, organizationId, createdBy);
            Console.WriteLine($"✓ Payment created: $15,000.00 (Full payment)");

            // === Invoice 2: Website Redesign - Development Phase ===
            var invoice2Id = await transactionService.CreateInvoiceAsync(new CreateInvoiceDto
            {
                ClientId = client1Id,
                ProjectId = project1Id,
                Amount = 25000.00m,
                Reference = "Website Redesign - Phase 2: Development & Integration",
                Date = DateTime.UtcNow.AddDays(-20),
                Lines = new List<CreateInvoiceLineDto>
                {
                    new CreateInvoiceLineDto{
                        Description = "Frontend development (HTML/CSS/JavaScript)",
                        Quantity = 80,
                        UnitPrice = 150.00m
                    },
                    new CreateInvoiceLineDto{
                        Description = "Backend development and API integration",
                        Quantity = 60,
                        UnitPrice = 175.00m
                    },
                    new CreateInvoiceLineDto{
                        Description = "CMS setup and configuration",
                        Quantity = 1,
                        UnitPrice = 2500.00m
                    }
                }
            }, organizationId, createdBy);
            Console.WriteLine($"✓ Invoice created: Website Redesign Phase 2 ($25,000.00)");

            // Early payment discount on Invoice 2
            await transactionService.ApplyDiscountAsync(new CreateDiscountDto
            {
                ClientId = client1Id,
                ProjectId = project1Id,
                Amount = 1250.00m,
                Reason = "5% early payment discount",
                Date = DateTime.UtcNow.AddDays(-18),
            }, organizationId, createdBy);
            Console.WriteLine($"✓ Discount created: $1,250.00 (Early payment discount)");

            // Partial payment for Invoice 2
            await transactionService.CreateProjectPaymentAsync(new CreateProjectPaymentDto
            {
                ProjectId = project1Id,
                Amount = 20000.00m,
                Reference = "Partial payment for Website Redesign Phase 2",
                Date = DateTime.UtcNow.AddDays(-15),
            }, organizationId, createdBy);
            Console.WriteLine($"✓ Payment created: $20,000.00 (Partial payment)");

            // === Invoice 3: Mobile App Development - Initial Milestone ===
            var invoice3Id = await transactionService.CreateInvoiceAsync(new CreateInvoiceDto
            {
                ClientId = client1Id,
                ProjectId = project2Id,
                Amount = 35000.00m,
                Reference = "Mobile App Development - Milestone 1: iOS App Foundation",
                Date = DateTime.UtcNow.AddDays(-35),
                Lines = new List<CreateInvoiceLineDto>
                {
                    new CreateInvoiceLineDto{
                        Description = "iOS app architecture and setup",
                        Quantity = 40,
                        UnitPrice = 200.00m
                    },
                    new CreateInvoiceLineDto{
                        Description = "Core feature development (authentication, navigation)",
                        Quantity = 75,
                        UnitPrice = 200.00m
                    },
                    new CreateInvoiceLineDto{
                        Description = "UI/UX implementation and polish",
                        Quantity = 50,
                        UnitPrice = 150.00m
                    },
                    new CreateInvoiceLineDto{
                        Description = "Testing and bug fixes",
                        Quantity = 20,
                        UnitPrice = 175.00m
                    }
                }
            }, organizationId, createdBy);
            Console.WriteLine($"✓ Invoice created: Mobile App Milestone 1 ($35,000.00)");

            // Full payment for Mobile App Invoice 1
            await transactionService.CreateProjectPaymentAsync(new CreateProjectPaymentDto
            {
                ProjectId = project2Id,
                Amount = 35000.00m,
                Reference = "Payment for Mobile App Milestone 1",
                Date = DateTime.UtcNow.AddDays(-28),
            }, organizationId, createdBy);
            Console.WriteLine($"✓ Payment created: $35,000.00 (Full payment)");

            // === Invoice 4: Mobile App Development - Android Development ===
            var invoice4Id = await transactionService.CreateInvoiceAsync(new CreateInvoiceDto
            {
                ClientId = client1Id,
                ProjectId = project2Id,
                Amount = 30000.00m,
                Reference = "Mobile App Development - Milestone 2: Android App Development",
                Date = DateTime.UtcNow.AddDays(-12),
                Lines = new List<CreateInvoiceLineDto>
                {
                    new CreateInvoiceLineDto{
                        Description = "Android app architecture and setup",
                        Quantity = 40,
                        UnitPrice = 200.00m
                    },
                    new CreateInvoiceLineDto{
                        Description = "Feature parity development",
                        Quantity = 65,
                        UnitPrice = 200.00m
                    },
                    new CreateInvoiceLineDto{
                        Description = "Cross-platform testing and optimization",
                        Quantity = 50,
                        UnitPrice = 180.00m
                    }
                }
            }, organizationId, createdBy);
            Console.WriteLine($"✓ Invoice created: Mobile App Milestone 2 ($30,000.00)");

            // Adjustment for extra features added to Mobile App
            await transactionService.CreateAdjustmentAsync(new CreateAdjustmentDto
            {
                ClientId = client1Id,
                ProjectId = project2Id,
                Amount = 2500.00m,
                Reason = "Additional push notification features requested mid-development",
                Date = DateTime.UtcNow.AddDays(-8),
                IsPositive = true
            }, organizationId, createdBy);
            Console.WriteLine($"✓ Adjustment created: +$2,500.00 (Additional features)");

            // === Invoice 5: TechStart Monthly Consulting - January ===
            var invoice5Id = await transactionService.CreateInvoiceAsync(new CreateInvoiceDto
            {
                ClientId = client2Id,
                ProjectId = project3Id,
                Amount = 5000.00m,
                Reference = "Monthly Consulting - January 2024",
                Date = DateTime.UtcNow.AddDays(-60),
                Lines = new List<CreateInvoiceLineDto>
                {
                    new CreateInvoiceLineDto{
                        Description = "Technical consulting and advisory services",
                        Quantity = 20,
                        UnitPrice = 175.00m
                    },
                    new CreateInvoiceLineDto{
                        Description = "Code review and architecture guidance",
                        Quantity = 10,
                        UnitPrice = 150.00m
                    }
                }
            }, organizationId, createdBy);
            Console.WriteLine($"✓ Invoice created: TechStart January Consulting ($5,000.00)");

            // Payment for January consulting
            await transactionService.CreateProjectPaymentAsync(new CreateProjectPaymentDto
            {
                ProjectId = project3Id,
                Amount = 5000.00m,
                Reference = "Payment for January consulting services",
                Date = DateTime.UtcNow.AddDays(-55),
            }, organizationId, createdBy);
            Console.WriteLine($"✓ Payment created: $5,000.00 (January consulting)");

            // === Invoice 6: TechStart Monthly Consulting - February ===
            var invoice6Id = await transactionService.CreateInvoiceAsync(new CreateInvoiceDto
            {
                ClientId = client2Id,
                ProjectId = project3Id,
                Amount = 5000.00m,
                Reference = "Monthly Consulting - February 2024",
                Date = DateTime.UtcNow.AddDays(-30),
                Lines = new List<CreateInvoiceLineDto>
                {
                    new CreateInvoiceLineDto{
                        Description = "Technical consulting and advisory services",
                        Quantity = 20,
                        UnitPrice = 175.00m
                    },
                    new CreateInvoiceLineDto{
                        Description = "Performance optimization consulting",
                        Quantity = 10,
                        UnitPrice = 150.00m
                    }
                }
            }, organizationId, createdBy);
            Console.WriteLine($"✓ Invoice created: TechStart February Consulting ($5,000.00)");

            // Payment for February consulting
            await transactionService.CreateProjectPaymentAsync(new CreateProjectPaymentDto
            {
                ProjectId = project3Id,
                Amount = 5000.00m,
                Reference = "Payment for February consulting services",
                Date = DateTime.UtcNow.AddDays(-25),
            }, organizationId, createdBy);
            Console.WriteLine($"✓ Payment created: $5,000.00 (February consulting)");

            // === Invoice 7: TechStart Monthly Consulting - March (Current) ===
            var invoice7Id = await transactionService.CreateInvoiceAsync(new CreateInvoiceDto
            {
                ClientId = client2Id,
                ProjectId = project3Id,
                Amount = 5500.00m,
                Reference = "Monthly Consulting - March 2024 (includes emergency support)",
                Date = DateTime.UtcNow.AddDays(-5),
                Lines = new List<CreateInvoiceLineDto>
                {
                    new CreateInvoiceLineDto{
                        Description = "Technical consulting and advisory services",
                        Quantity = 20,
                        UnitPrice = 175.00m
                    },
                    new CreateInvoiceLineDto{
                        Description = "Emergency weekend production support",
                        Quantity = 6,
                        UnitPrice = 250.00m
                    },
                    new CreateInvoiceLineDto{
                        Description = "Security audit and recommendations",
                        Quantity = 5,
                        UnitPrice = 100.00m
                    }
                }
            }, organizationId, createdBy);
            Console.WriteLine($"✓ Invoice created: TechStart March Consulting ($5,500.00)");

            // Credit note for unused hours from previous month
            await transactionService.CreateAdjustmentAsync(new CreateAdjustmentDto
            {
                ClientId = client2Id,
                ProjectId = project3Id,
                Amount = 500.00m,
                Reason = "Credit note - Unused consulting hours rollover from February",
                Date = DateTime.UtcNow.AddDays(-3),
                IsPositive = false
            }, organizationId, createdBy);
            Console.WriteLine($"✓ Adjustment created: -$500.00 (Credit Note)");

            // Volume discount for TechStart (loyal client)
            await transactionService.ApplyDiscountAsync(new CreateDiscountDto
            {
                ClientId = client2Id,
                ProjectId = project3Id,
                Amount = 250.00m,
                Reason = "Volume discount - 3+ months of continued service",
                Date = DateTime.UtcNow.AddDays(-2),
            }, organizationId, createdBy);
            Console.WriteLine($"✓ Discount created: $250.00 (Volume discount)");

            // Adjustment: Correction for overbilling on Website project
            await transactionService.CreateAdjustmentAsync(new CreateAdjustmentDto
            {
                ClientId = client1Id,
                ProjectId = project1Id,
                Amount = 750.00m,
                Reason = "Billing correction - Duplicate hours removed",
                Date = DateTime.UtcNow.AddDays(-7),
                IsPositive = false
            }, organizationId, createdBy);
            Console.WriteLine($"✓ Adjustment created: -$750.00 (Billing correction)");

            // Referral discount for Acme Corporation
            await transactionService.ApplyDiscountAsync(new CreateDiscountDto
            {
                ClientId = client1Id,
                ProjectId = project2Id,
                Amount = 1000.00m,
                Reason = "Referral discount - Thank you for referring TechStart Inc",
                Date = DateTime.UtcNow.AddDays(-10),
            }, organizationId, createdBy);
            Console.WriteLine($"✓ Discount created: $1,000.00 (Referral discount)");
        }
    }
}