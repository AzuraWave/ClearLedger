using ApplicationLayer.DTOs.Transactions;
using ApplicationLayer.Interfaces.Services;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Services
{
    public class StatementExcelGenerator : IStatementExcelGenerator
    {
        private readonly ILogger<StatementExcelGenerator> _logger;

        public StatementExcelGenerator(ILogger<StatementExcelGenerator> logger)
        {
            _logger = logger;
        }


        public byte[] Generate(StatementOfAccountDto statement)
        {

            _logger.LogInformation(
        "Generating Excel statement for ClientId {ClientId} from {From} to {To}. TransactionCount: {Count}",
        statement.ClientId,
        statement.From,
        statement.To,
        statement.Transactions?.Count ?? 0);
            try
            {
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Statement");

                ws.Cell(1, 1).Value = $"Statement of Account for {statement.ClientName}";
                ws.Range(1, 1, 1, 5).Merge().Style.Font.Bold = true;

                ws.Cell(2, 1).Value = $"Period: {statement.From:yyyy-MM-dd} to {statement.To:yyyy-MM-dd}";
                ws.Range(2, 1, 2, 5).Merge();

                ws.Cell(3, 1).Value = $"Opening Balance: {statement.OpeningBalance:C}";
                ws.Range(3, 1, 3, 5).Merge();


                ws.Cell(5, 1).Value = "Date";
                ws.Cell(5, 2).Value = "Reference";
                ws.Cell(5, 3).Value = "Type";
                ws.Cell(5, 4).Value = "Project";
                ws.Cell(5, 5).Value = "Amount";
                ws.Cell(5, 6).Value = "Running Balance";
                ws.Range(5, 1, 5, 6).Style.Font.Bold = true;

                int row = 6;
                decimal runningBalance = statement.OpeningBalance;

                foreach (var t in statement.Transactions)
                {
                    runningBalance += t.Amount;

                    ws.Cell(row, 1).Value = t.Date.ToString("yyyy-MM-dd");
                    ws.Cell(row, 2).Value = t.Reference;
                    ws.Cell(row, 3).Value = t.Type.ToString();
                    ws.Cell(row, 4).Value = t.ProjectName;      // populate project
                    ws.Cell(row, 5).Value = t.Amount;
                    ws.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
                    ws.Cell(row, 6).Value = runningBalance;
                    ws.Cell(row, 6).Style.NumberFormat.Format = "$#,##0.00";

                    row++;
                }

                // Closing balance
                ws.Cell(row, 5).Value = "Closing Balance";
                ws.Cell(row, 5).Style.Font.Bold = true;
                ws.Cell(row, 6).Value = statement.ClosingBalance;
                ws.Cell(row, 6).Style.Font.Bold = true;
                ws.Cell(row, 6).Style.NumberFormat.Format = "$#,##0.00";

                ws.Columns().AdjustToContents();

                using var ms = new MemoryStream();
                workbook.SaveAs(ms);
                var result = ms.ToArray();

                _logger.LogInformation(
                    "Excel statement generated successfully for ClientId {ClientId}. Size: {SizeBytes} bytes",
                    statement.ClientId,
                    result.Length);

                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to generate Excel statement for ClientId {ClientId}",
                    statement.ClientId);

                throw;
            }
        }
    }
}
