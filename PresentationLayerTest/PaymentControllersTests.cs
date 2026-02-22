using ApplicationLayer.DTOs.Transactions.Payments;
using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PresentationLayerTest
{
    public class PaymentControllersTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public PaymentControllersTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();

            _client.DefaultRequestHeaders.Add("X-API-KEY", "ApiKey " + Seeding.APIKEY);
            _client.DefaultRequestHeaders.Add("X-Test-OrgId", Seeding.ORG_ID.ToString());
        }

        #region ClientPayment Tests

        [Fact]
        public async Task GetClientPayment_Success()
        {
            var response = await _client.GetAsync($"/api/v1/payments/client/{Seeding.PAYMENT_ID}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetClientPayment_WithInvalidId_ReturnsNotFound()
        {
            var invalidPaymentId = Guid.NewGuid();
            var response = await _client.GetAsync($"/api/v1/payments/client/{invalidPaymentId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetClientPayment_WithoutApiKey_ReturnsUnauthorized()
        {
            using var clientWithoutApiKey = _factory.CreateClient();

            var response = await clientWithoutApiKey.GetAsync($"/api/v1/payments/client/{Seeding.PAYMENT_ID}");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateClientPayment_Success()
        {
            var createDto = new CreateClientPaymentDto
            {
                ClientId = Seeding.CLIENT_ID,
                TotalAmount = 500m,
                Date = DateTime.UtcNow,
                Reference = "Test Payment",
                Allocations = new List<PaymentAllocationDto>
                {
                    new PaymentAllocationDto
                    {
                        Id = Seeding.PROJECT_ID,
                        ProjectId = Seeding.PROJECT_ID,
                        Name = "Test Project",
                        Amount = 500m,
                        status = ProjectStatus.Active,
                        IsSelected = true
                    }
                }
            };

            var response = await _client.PostAsJsonAsync("/api/v1/payments/client", createDto);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(result.TryGetProperty("paymentId", out var paymentIdProperty));
            var paymentId = paymentIdProperty.GetGuid();
            Assert.NotEqual(Guid.Empty, paymentId);
        }

        [Fact]
        public async Task CreateClientPayment_WithoutApiKey_ReturnsUnauthorized()
        {
            using var clientWithoutApiKey = _factory.CreateClient();

            var createDto = new CreateClientPaymentDto
            {
                ClientId = Seeding.CLIENT_ID,
                TotalAmount = 500m,
                Date = DateTime.UtcNow
            };

            var response = await clientWithoutApiKey.PostAsJsonAsync("/api/v1/payments/client", createDto);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateClientPayment_WithMultipleAllocations_Success()
        {
            var createDto = new CreateClientPaymentDto
            {
                ClientId = Seeding.CLIENT_ID,
                TotalAmount = 1000m,
                Date = DateTime.UtcNow,
                Reference = "Multi-Project Payment",
                Allocations = new List<PaymentAllocationDto>
                {
                    new PaymentAllocationDto
                    {
                        Id = Seeding.PROJECT_ID,
                        ProjectId = Seeding.PROJECT_ID,
                        Name = "Project 1",
                        Amount = 600m,
                        status = ProjectStatus.Active,
                        IsSelected = true
                    },
                    new PaymentAllocationDto
                    {
                        Id = Seeding.PROJECT_ID,
                        ProjectId = Seeding.PROJECT_ID,
                        Name = "Project 2",
                        Amount = 400m,
                        status = ProjectStatus.Active,
                        IsSelected = true
                    }
                }
            };

            var response = await _client.PostAsJsonAsync("/api/v1/payments/client", createDto);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion

        #region ProjectPayment Tests

        [Fact]
        public async Task GetProjectPayment_Success()
        {
            var response = await _client.GetAsync($"/api/v1/payments/project/{Seeding.PAYMENT_ID}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetProjectPayment_WithInvalidId_ReturnsNotFound()
        {
            var invalidPaymentId = Guid.NewGuid();
            var response = await _client.GetAsync($"/api/v1/payments/project/{invalidPaymentId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetProjectPayment_WithoutApiKey_ReturnsUnauthorized()
        {
            using var clientWithoutApiKey = _factory.CreateClient();

            var response = await clientWithoutApiKey.GetAsync($"/api/v1/payments/project/{Seeding.PAYMENT_ID}");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateProjectPayment_Success()
        {
            var createDto = new CreateProjectPaymentDto
            {
                ProjectId = Seeding.PROJECT_ID,
                Amount = 750m,
                Date = DateTime.UtcNow,
                Reference = "Test Project Payment"
            };

            var response = await _client.PostAsJsonAsync("/api/v1/payments/project", createDto);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(result.TryGetProperty("paymentId", out var paymentIdProperty));
            var paymentId = paymentIdProperty.GetGuid();
            Assert.NotEqual(Guid.Empty, paymentId);
        }

        [Fact]
        public async Task CreateProjectPayment_WithoutApiKey_ReturnsUnauthorized()
        {
            using var clientWithoutApiKey = _factory.CreateClient();

            var createDto = new CreateProjectPaymentDto
            {
                ProjectId = Seeding.PROJECT_ID,
                Amount = 750m,
                Date = DateTime.UtcNow
            };

            var response = await clientWithoutApiKey.PostAsJsonAsync("/api/v1/payments/project", createDto);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateProjectPayment_WithReference_Success()
        {
            var createDto = new CreateProjectPaymentDto
            {
                ProjectId = Seeding.PROJECT_ID,
                Amount = 1200m,
                Date = DateTime.UtcNow,
                Reference = "Monthly Retainer Payment - December 2024"
            };

            var response = await _client.PostAsJsonAsync("/api/v1/payments/project", createDto);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion
    }
}
