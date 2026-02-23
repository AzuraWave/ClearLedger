using ApplicationLayer.DTOs.Transactions;
using ApplicationLayer.Interfaces.Services;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Services
{
    public class StatementPdfGenerator : IStatementDocumentGenerator
    {
        private readonly ILogger<StatementPdfGenerator> _logger;

        // Color scheme
        private static readonly string PrimaryColor = "#2C3E50";
        private static readonly string AccentColor = "#3498DB";
        private static readonly string LightGray = "#ECF0F1";
        private static readonly string DarkGray = "#7F8C8D";
        private static readonly string PositiveColor = "#27AE60";
        private static readonly string NegativeColor = "#E74C3C";

        public StatementPdfGenerator(ILogger<StatementPdfGenerator> logger)
        {
            _logger = logger;
        }

        public byte[] Generate(StatementOfAccountDto statement)
        {
            _logger.LogInformation(
                "Generating statement PDF for ClientId {ClientId} from {From} to {To}. TransactionCount: {Count}",
                statement.ClientId,
                statement.From,
                statement.To,
                statement.Transactions?.Count ?? 0);

            try
            {
                var pdf = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(50);
                        page.Size(PageSizes.A4);

                        // Header
                        page.Header().Column(column =>
                        {
                            column.Item().Background(PrimaryColor).Padding(20).Column(headerCol =>
                            {
                                headerCol.Item().Text("STATEMENT OF ACCOUNT")
                                    .FontSize(24)
                                    .Bold()
                                    .FontColor(Colors.White);

                                headerCol.Item().PaddingTop(5).Text(statement.ClientName)
                                    .FontSize(16)
                                    .FontColor(Colors.White);
                            });

                            column.Item().PaddingTop(20).Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Background(LightGray).Padding(15).Column(infoCol =>
                                    {
                                        infoCol.Item().Text("Statement Period")
                                            .FontSize(10)
                                            .FontColor(DarkGray)
                                            .SemiBold();

                                        infoCol.Item().PaddingTop(5).Text($"{statement.From:MMM dd, yyyy} - {statement.To:MMM dd, yyyy}")
                                            .FontSize(12)
                                            .FontColor(PrimaryColor)
                                            .SemiBold();
                                    });
                                });

                                row.ConstantItem(20);

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Background(LightGray).Padding(15).Column(infoCol =>
                                    {
                                        infoCol.Item().Text("Opening Balance")
                                            .FontSize(10)
                                            .FontColor(DarkGray)
                                            .SemiBold();

                                        infoCol.Item().PaddingTop(5).Text(statement.OpeningBalance.ToString("C"))
                                            .FontSize(12)
                                            .FontColor(statement.OpeningBalance >= 0 ? PositiveColor : NegativeColor)
                                            .SemiBold();
                                    });
                                });
                            });
                        });

                        // Content
                        page.Content().PaddingTop(20).Column(col =>
                        {
                            col.Item().Text("Transaction History")
                                .FontSize(16)
                                .FontColor(PrimaryColor)
                                .SemiBold();

                            col.Item().PaddingTop(15).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(85);   // Date
                                    columns.RelativeColumn(2);    // Project
                                    columns.RelativeColumn(2);    // Description
                                    columns.ConstantColumn(85);   // Debit
                                    columns.ConstantColumn(85);   // Credit
                                    columns.ConstantColumn(95);   // Balance
                                });

                                // Table header
                                table.Header(header =>
                                {
                                    header.Cell().Background(AccentColor).Padding(8)
                                        .Text("Date").FontSize(10).Bold().FontColor(Colors.White);

                                    header.Cell().Background(AccentColor).Padding(8)
                                        .Text("Project").FontSize(10).Bold().FontColor(Colors.White);

                                    header.Cell().Background(AccentColor).Padding(8)
                                        .Text("Type").FontSize(10).Bold().FontColor(Colors.White);

                                    header.Cell().Background(AccentColor).Padding(8)
                                        .AlignRight().Text("Debit").FontSize(10).Bold().FontColor(Colors.White);

                                    header.Cell().Background(AccentColor).Padding(8)
                                        .AlignRight().Text("Credit").FontSize(10).Bold().FontColor(Colors.White);

                                    header.Cell().Background(AccentColor).Padding(8)
                                        .AlignRight().Text("Balance").FontSize(10).Bold().FontColor(Colors.White);
                                });

                                decimal runningBalance = statement.OpeningBalance;
                                int rowIndex = 0;

                                foreach (var tx in statement.Transactions.OrderBy(t => t.Date))
                                {
                                    runningBalance += tx.Amount;
                                    var bgColor = rowIndex % 2 == 0 ? Colors.White.ToString() : LightGray;
                                    rowIndex++;

                                    table.Cell().Background(bgColor).Padding(8) 
                                        .Text(tx.Date.ToString("MMM dd, yyyy")).FontSize(9);

                                    table.Cell().Background(bgColor).Padding(8)
                                        .Text(tx.ProjectName ?? "-").FontSize(9);

                                    table.Cell().Background(bgColor).Padding(8)
                                        .Text(tx.Type.ToString()).FontSize(9);

                                    table.Cell().Background(bgColor).Padding(8)
                                        .AlignRight()
                                        .Text(tx.Amount > 0 ? tx.Amount.ToString("C") : "-")
                                        .FontSize(9)
                                        .FontColor(tx.Amount > 0 ? NegativeColor : PrimaryColor);

                                    table.Cell().Background(bgColor).Padding(8)
                                        .AlignRight()
                                        .Text(tx.Amount < 0 ? Math.Abs(tx.Amount).ToString("C") : "-")
                                        .FontSize(9)
                                        .FontColor(tx.Amount < 0 ? PositiveColor : PrimaryColor);

                                    table.Cell().Background(bgColor).Padding(8)
                                        .AlignRight()
                                        .Text(runningBalance.ToString("C"))
                                        .FontSize(9)
                                        .Bold()
                                        .FontColor(runningBalance >= 0 ? PositiveColor : NegativeColor);
                                }

                                // Closing balance row
                                table.Cell().ColumnSpan(5).Background(PrimaryColor).Padding(10)
                                    .AlignRight()
                                    .Text("Closing Balance")
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor(Colors.White);

                                table.Cell().Background(PrimaryColor).Padding(10)
                                    .AlignRight()
                                    .Text(runningBalance.ToString("C"))
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor(Colors.White);
                            });

                            // Summary section
                            col.Item().PaddingTop(20).Row(row =>
                            {
                                var totalDebits = statement.Transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
                                var totalCredits = Math.Abs(statement.Transactions.Where(t => t.Amount < 0).Sum(t => t.Amount));

                                row.RelativeItem().Background(LightGray).Padding(15).Column(summaryCol =>
                                {
                                    summaryCol.Item().Text("Total Debits")
                                        .FontSize(10)
                                        .FontColor(DarkGray);

                                    summaryCol.Item().PaddingTop(5).Text(totalDebits.ToString("C"))
                                        .FontSize(12)
                                        .FontColor(NegativeColor)
                                        .SemiBold();
                                });

                                row.ConstantItem(15);

                                row.RelativeItem().Background(LightGray).Padding(15).Column(summaryCol =>
                                {
                                    summaryCol.Item().Text("Total Credits")
                                        .FontSize(10)
                                        .FontColor(DarkGray);

                                    summaryCol.Item().PaddingTop(5).Text(totalCredits.ToString("C"))
                                        .FontSize(12)
                                        .FontColor(PositiveColor)
                                        .SemiBold();
                                });

                                row.ConstantItem(15);

                                row.RelativeItem().Background(LightGray).Padding(15).Column(summaryCol =>
                                {
                                    summaryCol.Item().Text("Net Change")
                                        .FontSize(10)
                                        .FontColor(DarkGray);

                                    var netChange = totalDebits - totalCredits;
                                    summaryCol.Item().PaddingTop(5).Text(netChange.ToString("C"))
                                        .FontSize(12)
                                        .FontColor(netChange >= 0 ? NegativeColor : PositiveColor)
                                        .SemiBold();
                                });
                            });
                        });

                        // Footer
                        page.Footer().Column(column =>
                        {
                            column.Item().BorderTop(1).BorderColor(DarkGray).PaddingTop(10);

                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"Generated on {DateTime.Now:MMM dd, yyyy 'at' hh:mm tt}")
                                    .FontSize(9)
                                    .FontColor(DarkGray);

                                row.RelativeItem().AlignRight().Text(text =>
                                {
                                    text.Span("Page ").FontSize(9).FontColor(DarkGray);
                                    text.CurrentPageNumber().FontSize(9).FontColor(DarkGray);
                                    text.Span(" of ").FontSize(9).FontColor(DarkGray);
                                    text.TotalPages().FontSize(9).FontColor(DarkGray);
                                });
                            });
                        });
                    });
                }).GeneratePdf();

                _logger.LogInformation(
                    "Statement PDF generated successfully for ClientId {ClientId}",
                    statement.ClientId);

                return pdf;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to generate statement PDF for ClientId {ClientId}",
                    statement.ClientId);

                throw;
            }
        }
    }
}
