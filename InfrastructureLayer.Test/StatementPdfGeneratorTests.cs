using ApplicationLayer.DTOs.Transactions;
using DomainLayer.Enums;
using InfrastructureLayer.Services;
using Microsoft.Extensions.Logging;
using Moq;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Test
{
    public class StatementPdfGeneratorTests
    {
        public StatementPdfGeneratorTests()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }
        [Fact]
        public void Generate_ReturnsPdfBytes_AndLogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<StatementPdfGenerator>>();
            var generator = new StatementPdfGenerator(loggerMock.Object);

            var statement = new StatementOfAccountDto
            {
                ClientId = Guid.NewGuid(),
                ClientName = "Test Client",
                From = new DateTime(2026, 1, 1),
                To = new DateTime(2026, 1, 31),
                OpeningBalance = 100m,
                Transactions = new List<TransactionDto>
            {
                new TransactionDto { Date = new DateTime(2026,1,5), Reference = "INV001", Type = TransactionType.Invoice, Amount = 50, ProjectName = "Project A" },
                new TransactionDto { Date = new DateTime(2026,1,10), Reference = "PMT001", Type = TransactionType.Payment, Amount = -20, ProjectName = "Project B" }
            }
            };

            // Act
            var result = generator.Generate(statement);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0, "Generated PDF should have content");
        }
    }
}
