using ApplicationLayer.DTOs.Client;
using ApplicationLayer.DTOs.Transactions.Invoices;
using ApplicationLayer.Interfaces.Services;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Services
{
    public class InvoiceExcelGenerator : IInvoiceExcelGenerator
    {
        private readonly ILogger<InvoiceExcelGenerator> _logger;

        public InvoiceExcelGenerator(ILogger<InvoiceExcelGenerator> logger)
        {
            _logger = logger;
        }
        public byte[] Generate(InvoiceReadDto invoice, ClientReadDto client)
        {
            _logger.LogInformation(
        "Generating invoice Excel. InvoiceId {InvoiceId}, ClientId {ClientId}, LineCount {LineCount}",
        invoice.Id,
        client.Id,
        invoice.Lines?.Count ?? 0);
            
            try
            {
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Invoice");

                ws.Cell(1, 1).Value = "Invoice Number:";
                ws.Cell(1, 2).Value = invoice.InvoiceNumber;
                ws.Cell(2, 1).Value = "Date:";
                ws.Cell(2, 2).Value = invoice.Date.ToString("yyyy-MM-dd");
                ws.Cell(3, 1).Value = "Client:";
                ws.Cell(3, 2).Value = invoice.ClientName;

                ws.Cell(4, 2).Value = client.Address ?? "No address";
                ws.Cell(5, 2).Value = client.PhoneNumber ?? "No phone";

                ws.Cell(6, 1).Value = "Project:";
                ws.Cell(6, 2).Value = invoice.ProjectName;

                ws.Cell(8, 1).Value = "Description";
                ws.Cell(8, 2).Value = "Quantity";
                ws.Cell(8, 3).Value = "Unit Price";
                ws.Cell(8, 4).Value = "Line Total";


                int row = 9;
                foreach (var line in invoice.Lines)
                {
                    ws.Cell(row, 1).Value = line.Description;
                    ws.Cell(row, 2).Value = line.Quantity;
                    ws.Cell(row, 3).Value = line.UnitPrice;
                    ws.Cell(row, 4).Value = line.LineTotal;
                    row++;
                }

                ws.Cell(row, 3).Value = "Total:";
                ws.Cell(row, 4).Value = invoice.TotalAmount;

                using var ms = new MemoryStream();
                workbook.SaveAs(ms);
                var result = ms.ToArray();

                _logger.LogInformation(
                    "Invoice Excel generated successfully. InvoiceId {InvoiceId}, Size {SizeBytes} bytes",
                    invoice.Id,
                    result.Length);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to generate invoice Excel. InvoiceId {InvoiceId}",
                    invoice.Id);

                throw;
            }
        }
    }
}
