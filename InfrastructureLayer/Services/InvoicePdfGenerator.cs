using ApplicationLayer.DTOs.Client;
using ApplicationLayer.DTOs.Organization;
using ApplicationLayer.DTOs.Transactions.Invoices;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Entities;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Services
{
    public class InvoicePdfGenerator : IInvoiceDocumentGenerator
    {
        private readonly ILogger<InvoicePdfGenerator> _logger;

        public InvoicePdfGenerator(ILogger<InvoicePdfGenerator> logger)
        {
            _logger = logger;
        }
        public byte[] Generate(InvoiceReadDto invoice, ClientReadDto client, OrganizationReadDto org)
        {
            _logger.LogInformation(
            "Generating invoice PDF. InvoiceId {InvoiceId}, ClientId {ClientId}, OrganizationId {OrganizationId}, LineCount {LineCount}",
            invoice.Id,
            client.Id,
            org.Id,
            invoice.Lines?.Count ?? 0);
            try
            {
                var pdf = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(40);

                        page.Content().Column(col =>
                        {
                            // HEADER
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text(org.Name)
                                        .FontSize(18).SemiBold();


                                });

                                row.ConstantItem(200).AlignRight().Column(c =>
                                {
                                    c.Item().Text("INVOICE")
                                        .FontSize(20).Bold();

                                    c.Item().Text($"Invoice #: {invoice.InvoiceNumber}");
                                    c.Item().Text($"Date: {invoice.Date:yyyy-MM-dd}");

                                    if (!string.IsNullOrWhiteSpace(invoice.ProjectName))
                                    {
                                        c.Item().Text($"Project: {invoice.ProjectName}");
                                    }
                                    else
                                    {
                                        c.Item().Text(" No project found");
                                    }
                                });
                            });

                            col.Item().PaddingVertical(15).LineHorizontal(1);

                            // BILL TO
                            col.Item().Text("Bill To:")
                                .SemiBold();

                            col.Item().Text(invoice.ClientName);
                            col.Item().Text(client.Address ?? "No address");
                            col.Item().Text(client.PhoneNumber ?? "No PhoneNumber");

                            col.Item().PaddingVertical(15).LineHorizontal(1);

                            // LINE ITEMS
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Text("Description").SemiBold();
                                    header.Cell().AlignRight().Text("Qty").SemiBold();
                                    header.Cell().AlignRight().Text("Unit").SemiBold();
                                    header.Cell().AlignRight().Text("Total").SemiBold();
                                });

                                foreach (var line in invoice.Lines)
                                {
                                    table.Cell().Text(line.Description);
                                    table.Cell().AlignRight().Text(line.Quantity.ToString());
                                    table.Cell().AlignRight().Text(line.UnitPrice.ToString("C"));
                                    table.Cell().AlignRight().Text(line.LineTotal.ToString("C"));
                                }
                            });

                            col.Item().PaddingVertical(15).LineHorizontal(1);

                            // TOTAL
                            col.Item().AlignRight().Text($"Total: {invoice.TotalAmount:C}")
                                .FontSize(14).Bold();
                        });
                    });
                }).GeneratePdf();
                _logger.LogInformation(
                "Invoice PDF generated successfully. InvoiceId {InvoiceId}, Size {SizeBytes} bytes",
                invoice.Id,
                pdf.Length);

                return pdf;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to generate invoice PDF. InvoiceId {InvoiceId}",
                    invoice.Id);

                throw;
            }
        }
    }
}
