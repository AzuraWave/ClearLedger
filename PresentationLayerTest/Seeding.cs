using DocumentFormat.OpenXml.Office2010.Excel;
using DomainLayer.Entities;
using DomainLayer.Enums;
using InfrastructureLayer.Context;
using InfrastructureLayer.Identity.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace PresentationLayerTest
{
    

    public class Seeding
    {
        public static Guid ORG_ID = Guid.NewGuid();
        public static string ORG_NAME = "ORGTESTING";

        public static Guid CLIENT_ID = Guid.NewGuid();
        public static string CLIENT_NAME = "Client Name";
        public static string Address = "Address";

        public static Guid PROJECT_ID = Guid.NewGuid();

        public static Guid INVOICE_ID = Guid.NewGuid();
        public static Guid PAYMENT_ID = Guid.NewGuid();
        public static Guid DISCOUNT_ID = Guid.NewGuid();
        public static Guid ADJUSTMENT_ID = Guid.NewGuid();
        public static Guid INVOICE_COUNTER_ID = Guid.NewGuid();

        public static Organization org;
        public static Client client;
        public static Project project;

        public static string APIKEY = "test-api-key";

        public static bool seeded = false;

        public static void InitializeTestDB( LedgerDbContext db)
        {

            if (seeded == true ) return;
            Setup(db);
            seeded = true;

        }

        private static void Setup(LedgerDbContext db)
        {
            var org = GetOrganization();
            var client = GetClient();
            var project = GetProject();

            db.Organizations.Add(org);
            db.Clients.Add(client);
            db.Projects.Add(project);
            db.Invoices.Add(GetInvoice());
            db.ClientPaymentHeaders.Add(GetPayment());
            db.Discounts.Add(GetDiscount());
            db.Adjustments.Add(GetAdjustment());

            db.SaveChanges();
        }

        private static Organization GetOrganization()
        {
            if (org == null)
                org = new Organization { Id = ORG_ID, Name = ORG_NAME,
                    ApiKeyHash = HashApiKey(APIKEY),
                    DefaultAutomationUserId = Guid.NewGuid()
                };
            return org;
        }
        private static string HashApiKey(string apiKey)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(apiKey));
            return Convert.ToBase64String(bytes);
        }
        private static Client GetClient()
        {
            if (client == null)
            {
                client = new Client
                {
                    Id = CLIENT_ID,
                    Name = CLIENT_NAME,
                    Address = Address,
                    OrganizationId = ORG_ID
                };
            }
            return client;
        }

        private static Project GetProject()
        {
            if (project == null)
                project = new Project
                {
                    Id = PROJECT_ID,
                    Name = "Project 1",
                    ClientId = CLIENT_ID,
                    OrganizationId = ORG_ID
                };
            return project;
        }

        private static Invoice GetInvoice()
        {
            var line = new InvoiceLine
            {
                Description = "Development Work",
                Quantity = 10,
                UnitPrice = 100,
                LineTotal = 1000
            };

            return new Invoice
            {
                Id = INVOICE_ID,
                OrganizationId = org.Id,
                ClientId = client.Id,
                ProjectId = project.Id,
                InvoiceNumber = "INV-001",
                Status = InvoiceStatus.Issued,
                Date = DateTime.UtcNow.AddDays(-10),
                TotalAmount = 1000,
                IsVoided = false,
                Lines = new List<InvoiceLine> { line }
            };
        }

        private static ClientPaymentHeader GetPayment()
        {
            return new ClientPaymentHeader
            {
                Id = PAYMENT_ID,
                OrganizationId = org.Id,
                ClientId = client.Id,
                Date = DateTime.UtcNow.AddDays(-5),
                TotalAmount = 1000,
                Reference = "PAY-001",
                IsVoided = false,
                Allocations = new List<ClientPaymentAllocation>
        {
            new ClientPaymentAllocation
            {
                ProjectId = PROJECT_ID,
                Amount = 1000
            }
        }
            };
        }

        private static Discount GetDiscount()
        {
            return new Discount
            {
                Id = DISCOUNT_ID,
                OrganizationId = org.Id,
                ClientId = client.Id,
                ProjectId = project.Id,
                Amount = 100,
                Date = DateTime.UtcNow.AddDays(-9),
                Reason = "Loyalty discount"
            };
        }

        private static Adjustment GetAdjustment()
        {
            return new Adjustment
            {
                Id = ADJUSTMENT_ID,
                OrganizationId = org.Id,
                ClientId = client.Id,
                ProjectId = project.Id,
                Amount = 50,
                Date = DateTime.UtcNow.AddDays(-8),
                Reason = "Manual correction",
                IsPositive = false,
                RelatedEntityId = INVOICE_ID,
                RelatedEntityType = nameof(Invoice)
            };
        }

    }
}
