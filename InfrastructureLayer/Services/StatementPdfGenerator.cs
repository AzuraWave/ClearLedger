using ApplicationLayer.DTOs.Transactions;
using ApplicationLayer.Interfaces.Services;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Services
{
    public class StatementPdfGenerator : IStatementDocumentGenerator
    {

        private readonly ILogger<StatementPdfGenerator> _logger;

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
                        page.Margin(40);

                        page.Header()
                            .Text($"Statement of Account: {statement.ClientName}")
                            .FontSize(20)
                            .SemiBold();

                        page.Content().Column(col =>
                        {
                            col.Item().Text($"Period: {statement.From:yyyy-MM-dd} to {statement.To:yyyy-MM-dd}");
                            col.Item().Text($"Opening Balance: {statement.OpeningBalance:C}");

                            col.Item().LineHorizontal(1);

                            // Table header
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(90);
                                    columns.ConstantColumn(100);
                                    columns.RelativeColumn(3);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(90);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Date").SemiBold();
                                    header.Cell().Text("Project").SemiBold();
                                    header.Cell().Text("Description").SemiBold();
                                    header.Cell().Text("Debit").SemiBold().AlignRight();
                                    header.Cell().Text("Credit").SemiBold().AlignRight();
                                    header.Cell().Text("Balance").SemiBold().AlignRight();
                                });

                                decimal runningBalance = statement.OpeningBalance;

                                foreach (var tx in statement.Transactions.OrderBy(t => t.Date))
                                {
                                    runningBalance += tx.Amount;

                                    table.Cell().Text(tx.Date.ToString("yyyy-MM-dd"));
                                    table.Cell().Text(tx.ProjectName);
                                    table.Cell().Text(tx.Type.ToString());
                                    table.Cell().Text(tx.Amount > 0 ? tx.Amount.ToString("C") : "").AlignRight();
                                    table.Cell().Text(tx.Amount < 0 ? Math.Abs(tx.Amount).ToString("C") : "").AlignRight();
                                    table.Cell().Text(runningBalance.ToString("C")).AlignRight();
                                }
                                table.Cell().ColumnSpan(4).AlignRight().Text("Closing Balance").SemiBold();
                                table.Cell().AlignRight().Text(runningBalance.ToString("C")).SemiBold();
                            });

                        });


                        page.Footer().AlignCenter().Text(x => x.Span($"{x.CurrentPageNumber()} / {x.TotalPages()}"));
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
