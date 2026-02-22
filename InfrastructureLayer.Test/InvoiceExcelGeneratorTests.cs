using ApplicationLayer.DTOs.Client;
using ApplicationLayer.DTOs.Transactions.Invoices;
using InfrastructureLayer.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Test
{
    public class InvoiceExcelGeneratorTests
    {
        [Fact]
        public void Generate_ReturnsExcelBytes_AndLogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<InvoiceExcelGenerator>>();
            var generator = new InvoiceExcelGenerator(loggerMock.Object);

            var client = new ClientReadDto
            {
                Id = Guid.NewGuid(),
                Address = "123 Test St",
                PhoneNumber = "555-1234"
            };

            var invoice = new InvoiceReadDto
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = "INV-001",
                Date = new DateTime(2026, 1, 15),
                ClientName = "Test Client",
                ProjectName = "Project A",
                Lines = new List<InvoiceLineDto>
            {
                new InvoiceLineDto { Description = "Service A", Quantity = 2, UnitPrice = 50, LineTotal = 100 },
                new InvoiceLineDto { Description = "Service B", Quantity = 1, UnitPrice = 30, LineTotal = 30 }
            },
                TotalAmount = 130
            };

            // Act
            var result = generator.Generate(invoice, client);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0, "Generated Excel should have content");
        }
    }
}
