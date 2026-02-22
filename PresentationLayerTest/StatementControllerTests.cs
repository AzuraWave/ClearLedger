using ApplicationLayer.DTOs.Transactions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using InfrastructureLayer.Context;
using DomainLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace PresentationLayerTest
{
    public class StatementControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public StatementControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();

            _client.DefaultRequestHeaders.Add("X-API-KEY", "ApiKey " + Seeding.APIKEY);
            _client.DefaultRequestHeaders.Add("X-Test-OrgId", Seeding.ORG_ID.ToString());
        }

        [Fact]
        public async Task GetClientStatement_Success()
        {
            var from = DateTime.UtcNow.AddMonths(-1);
            var to = DateTime.UtcNow;

            var response = await _client.GetAsync(
                $"/api/v1/statements/client/{Seeding.CLIENT_ID}?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetClientStatement_ReturnsCorrectData()
        {
            var from = DateTime.UtcNow.AddMonths(-1);
            var to = DateTime.UtcNow;

            var response = await _client.GetAsync(
                $"/api/v1/statements/client/{Seeding.CLIENT_ID}?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var statement = await response.Content.ReadFromJsonAsync<StatementOfAccountDto>();
            Assert.NotNull(statement);
            Assert.Equal(Seeding.CLIENT_ID, statement.ClientId);
            Assert.Equal(Seeding.CLIENT_NAME, statement.ClientName);
            Assert.NotNull(statement.Transactions);
        }

        [Fact]
        public async Task GetClientStatement_WithInvalidClientId_ReturnsNotFound()
        {
            var invalidClientId = Guid.NewGuid();
            var from = DateTime.UtcNow.AddMonths(-1);
            var to = DateTime.UtcNow;

            var response = await _client.GetAsync(
                $"/api/v1/statements/client/{invalidClientId}?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetClientStatement_WithoutApiKey_ReturnsUnauthorized()
        {
            using var clientWithoutApiKey = _factory.CreateClient();
            var from = DateTime.UtcNow.AddMonths(-1);
            var to = DateTime.UtcNow;

            var response = await clientWithoutApiKey.GetAsync(
                $"/api/v1/statements/client/{Seeding.CLIENT_ID}?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetClientStatement_WithDateRange_Success()
        {
            var from = new DateTime(2024, 1, 1);
            var to = new DateTime(2024, 12, 31);

            var response = await _client.GetAsync(
                $"/api/v1/statements/client/{Seeding.CLIENT_ID}?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var statement = await response.Content.ReadFromJsonAsync<StatementOfAccountDto>();
            Assert.NotNull(statement);
            Assert.Equal(from, statement.From);
            Assert.Equal(to, statement.To);
        }

        [Fact]
        public async Task GetClientStatement_VerifiesClosingBalanceCalculation()
        {
            var from = DateTime.UtcNow.AddMonths(-1);
            var to = DateTime.UtcNow;

            var response = await _client.GetAsync(
                $"/api/v1/statements/client/{Seeding.CLIENT_ID}?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var statement = await response.Content.ReadFromJsonAsync<StatementOfAccountDto>();
            Assert.NotNull(statement);

            var expectedClosingBalance = statement.OpeningBalance + 
                statement.Transactions.Sum(t => t.Amount);

            Assert.Equal(expectedClosingBalance, statement.ClosingBalance);
        }

        [Fact]
        public async Task GetClientStatement_ShortDateRange_Success()
        {
            var from = DateTime.UtcNow.AddDays(-7);
            var to = DateTime.UtcNow;

            var response = await _client.GetAsync(
                $"/api/v1/statements/client/{Seeding.CLIENT_ID}?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetClientStatement_LongDateRange_Success()
        {
            var from = DateTime.UtcNow.AddYears(-1);
            var to = DateTime.UtcNow;

            var response = await _client.GetAsync(
                $"/api/v1/statements/client/{Seeding.CLIENT_ID}?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    
   
}
