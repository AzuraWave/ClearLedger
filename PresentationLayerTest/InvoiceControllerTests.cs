using ApplicationLayer.DTOs.Transactions.Invoices;
using DomainLayer.Entities;
using InfrastructureLayer.Context;
using InfrastructureLayer.Identity.Roles;
using InfrastructureLayer.Identity.User;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit.Abstractions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;
namespace PresentationLayerTest
{
    public class InvoicesControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public InvoicesControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();

            _client.DefaultRequestHeaders.Add("X-API-KEY", "ApiKey " + Seeding.APIKEY);
            _client.DefaultRequestHeaders.Add("X-Test-OrgId", Seeding.ORG_ID.ToString());
        }

        [Fact]
        public async Task GetInvoice_Success()
        {
            var response = await _client.GetAsync($"/api/v1/invoices/{Seeding.INVOICE_ID}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetInvoice_WithInvalidId_ReturnsNotFound()
        {
            var invalidInvoiceId = Guid.NewGuid();
            var response = await _client.GetAsync($"/api/v1/invoices/{invalidInvoiceId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);  // Should be NotFound, not Unauthorized
        }

        [Fact]
        public async Task GetInvoice_WithoutApiKey_ReturnsUnauthorized()
        {
            using var clientWithoutApiKey = _factory.CreateClient();
            // Intentionally omit X-API-KEY header

            var response = await clientWithoutApiKey.GetAsync($"/api/v1/invoices/{Seeding.INVOICE_ID}");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetInvoice_ReturnsCorrectInvoiceData()
        {
            var response = await _client.GetAsync($"/api/v1/invoices/{Seeding.INVOICE_ID}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var invoice = await response.Content.ReadFromJsonAsync<InvoiceReadDto>();
            Assert.NotNull(invoice);
            Assert.Equal(Seeding.INVOICE_ID, invoice.Id);
            Assert.Equal("INV-001", invoice.InvoiceNumber);
            Assert.Equal(1000m, invoice.TotalAmount);
        }

        [Fact]
        public async Task CreateInvoice_Success()
        {
            var createDto = new CreateInvoiceDto
            {
                ProjectId = Seeding.PROJECT_ID,
                ClientId = Seeding.CLIENT_ID,
                Amount = 500m,
                Date = DateTime.UtcNow,
                Reference = "Test Invoice",
                Lines = new List<CreateInvoiceLineDto>
                {
                    new CreateInvoiceLineDto
                    {
                        Description = "Test Service",
                        Quantity = 2,
                        UnitPrice = 250m
                    }
                }
            };

            var response = await _client.PostAsJsonAsync("/api/v1/invoices", createDto);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(result.TryGetProperty("invoiceId", out var invoiceIdProperty));
            var invoiceId = invoiceIdProperty.GetGuid();
            Assert.NotEqual(Guid.Empty, invoiceId);
        }

        [Fact]
        public async Task CreateInvoice_WithoutApiKey_ReturnsUnauthorized()
        {
            using var clientWithoutApiKey = _factory.CreateClient();

            var createDto = new CreateInvoiceDto
            {
                ProjectId = Seeding.PROJECT_ID,
                ClientId = Seeding.CLIENT_ID,
                Amount = 500m,
                Date = DateTime.UtcNow
            };

            var response = await clientWithoutApiKey.PostAsJsonAsync("/api/v1/invoices", createDto);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateInvoice_WithMultipleLines_Success()
        {
            var createDto = new CreateInvoiceDto
            {
                ProjectId = Seeding.PROJECT_ID,
                ClientId = Seeding.CLIENT_ID,
                Amount = 1500m,
                Date = DateTime.UtcNow,
                Lines = new List<CreateInvoiceLineDto>
                {
                    new CreateInvoiceLineDto
                    {
                        Description = "Consulting Service",
                        Quantity = 10,
                        UnitPrice = 100m
                    },
                    new CreateInvoiceLineDto
                    {
                        Description = "Development Service",
                        Quantity = 5,
                        UnitPrice = 100m
                    }
                }
            };

            var response = await _client.PostAsJsonAsync("/api/v1/invoices", createDto);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
