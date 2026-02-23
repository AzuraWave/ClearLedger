using ApplicationLayer.DTOs.Client;
using ApplicationLayer.DTOs.Organization;
using ApplicationLayer.DTOs.Transactions.Invoices;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Entities;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Services
{
    public class InvoicePdfGenerator : IInvoiceDocumentGenerator
    {
        private readonly ILogger<InvoicePdfGenerator> _logger;

        // Color scheme
        private static readonly string PrimaryColor = "#2C3E50";
        private static readonly string AccentColor = "#3498DB";
        private static readonly string LightGray = "#ECF0F1";
        private static readonly string DarkGray = "#7F8C8D";
        private static readonly string SuccessColor = "#27AE60";

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
                        page.Margin(50);
                        page.Size(PageSizes.A4);

                        // Header
                        page.Header().Column(column =>
                        {
                            column.Item().Row(row =>
                            {
                                // Organization info
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text(org.Name)
                                        .FontSize(24)
                                        .Bold()
                                        .FontColor(PrimaryColor);

                                    col.Item().PaddingTop(10).Text(text =>
                                    {
                                        text.Span("From: ").FontSize(10).FontColor(DarkGray);
                                        text.Span(org.Name).FontSize(10).FontColor(PrimaryColor);
                                    });
                                });

                                // Invoice badge
                                row.ConstantItem(180).Column(col =>
                                {
                                    col.Item().Background(AccentColor).Padding(15).Column(invoiceInfo =>
                                    {
                                        invoiceInfo.Item().Text("INVOICE")
                                            .FontSize(20)
                                            .Bold()
                                            .FontColor(Colors.White)
                                            .AlignCenter();

                                        invoiceInfo.Item().PaddingTop(5).Text($"#{invoice.InvoiceNumber}")
                                            .FontSize(14)
                                            .Bold()
                                            .FontColor(Colors.White)
                                            .AlignCenter();
                                    });

                                    col.Item().PaddingTop(10).Background(LightGray).Padding(10).Column(dateInfo =>
                                    {
                                        dateInfo.Item().Text("Date")
                                            .FontSize(9)
                                            .FontColor(DarkGray);

                                        dateInfo.Item().Text(invoice.Date.ToString("MMM dd, yyyy"))
                                            .FontSize(11)
                                            .Bold()
                                            .FontColor(PrimaryColor);
                                    });
                                });
                            });

                            // Project info if available
                            if (!string.IsNullOrWhiteSpace(invoice.ProjectName))
                            {
                                column.Item().PaddingTop(15).Background(LightGray).Padding(12).Row(row =>
                                {
                                    row.AutoItem().Text("Project: ")
                                        .FontSize(10)
                                        .FontColor(DarkGray)
                                        .SemiBold();

                                    row.AutoItem().Text(invoice.ProjectName)
                                        .FontSize(10)
                                        .FontColor(PrimaryColor)
                                        .Bold();
                                });
                            }

                            // Reference if available
                            if (!string.IsNullOrWhiteSpace(invoice.Reference))
                            {
                                column.Item().PaddingTop(5).Text(text =>
                                {
                                    text.Span("Reference: ").FontSize(9).FontColor(DarkGray);
                                    text.Span(invoice.Reference).FontSize(9).FontColor(PrimaryColor).Italic();
                                });
                            }
                        });

                        // Content
                        page.Content().PaddingTop(30).Column(col =>
                        {
                            // Bill To section
                            col.Item().Background(LightGray).Padding(15).Column(billToCol =>
                            {
                                billToCol.Item().Text("BILL TO")
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor(PrimaryColor);

                                billToCol.Item().PaddingTop(8).Text(invoice.ClientName)
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(PrimaryColor);

                                if (!string.IsNullOrWhiteSpace(client.Address))
                                {
                                    billToCol.Item().PaddingTop(3).Text(client.Address)
                                        .FontSize(10)
                                        .FontColor(DarkGray);
                                }

                                if (!string.IsNullOrWhiteSpace(client.PhoneNumber))
                                {
                                    billToCol.Item().PaddingTop(2).Text($"Phone: {client.PhoneNumber}")
                                        .FontSize(10)
                                        .FontColor(DarkGray);
                                }

                                if (!string.IsNullOrWhiteSpace(client.BillingEmail))
                                {
                                    billToCol.Item().PaddingTop(2).Text($"Email: {client.BillingEmail}")
                                        .FontSize(10)
                                        .FontColor(DarkGray);
                                }
                            });

                            // Line items title
                            col.Item().PaddingTop(30).Text("Invoice Items")
                                .FontSize(14)
                                .FontColor(PrimaryColor)
                                .SemiBold();

                            // Line items table
                            col.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(4);      // Description
                                    columns.ConstantColumn(80);     // Qty
                                    columns.ConstantColumn(90);     // Unit Price
                                    columns.ConstantColumn(100);    // Total
                                });

                                // Table header
                                table.Header(header =>
                                {
                                    header.Cell().Background(PrimaryColor).Padding(10)
                                        .Text("Description").FontSize(10).Bold().FontColor(Colors.White);

                                    header.Cell().Background(PrimaryColor).Padding(10)
                                        .AlignRight().Text("Quantity").FontSize(10).Bold().FontColor(Colors.White);

                                    header.Cell().Background(PrimaryColor).Padding(10)
                                        .AlignRight().Text("Unit Price").FontSize(10).Bold().FontColor(Colors.White);

                                    header.Cell().Background(PrimaryColor).Padding(10)
                                        .AlignRight().Text("Line Total").FontSize(10).Bold().FontColor(Colors.White);
                                });

                                // Line items
                                int rowIndex = 0;
                                foreach (var line in invoice.Lines)
                                {
                                    var bgColor = rowIndex % 2 == 0 ? Colors.White.ToString() : LightGray;
                                    rowIndex++;

                                    table.Cell().Background(bgColor).Padding(10)
                                        .Text(line.Description).FontSize(10);

                                    table.Cell().Background(bgColor).Padding(10)
                                        .AlignRight().Text(line.Quantity.ToString("N2")).FontSize(10);

                                    table.Cell().Background(bgColor).Padding(10)
                                        .AlignRight().Text(line.UnitPrice.ToString("C")).FontSize(10);

                                    table.Cell().Background(bgColor).Padding(10)
                                        .AlignRight().Text(line.LineTotal.ToString("C")).FontSize(10).SemiBold();
                                }

                                // Subtotal row
                                table.Cell().ColumnSpan(3).Background(LightGray).Padding(10)
                                    .AlignRight()
                                    .Text("Subtotal")
                                    .FontSize(11)
                                    .SemiBold()
                                    .FontColor(PrimaryColor);

                                table.Cell().Background(LightGray).Padding(10)
                                    .AlignRight()
                                    .Text(invoice.TotalAmount.ToString("C"))
                                    .FontSize(11)
                                    .SemiBold()
                                    .FontColor(PrimaryColor);

                                // Total row (highlighted)
                                table.Cell().ColumnSpan(3).Background(SuccessColor).Padding(12)
                                    .AlignRight()
                                    .Text("TOTAL DUE")
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(Colors.White);

                                table.Cell().Background(SuccessColor).Padding(12)
                                    .AlignRight()
                                    .Text(invoice.TotalAmount.ToString("C"))
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.White);
                            });

                            // Payment terms or note
                            col.Item().PaddingTop(30).Background(LightGray).Padding(15).Column(noteCol =>
                            {
                                noteCol.Item().Text("Payment Terms")
                                    .FontSize(10)
                                    .FontColor(DarkGray)
                                    .SemiBold();

                                noteCol.Item().PaddingTop(5).Text("Please remit payment within 30 days of invoice date.")
                                    .FontSize(9)
                                    .FontColor(DarkGray);
                            });
                        });

                        // Footer
                        page.Footer().Column(column =>
                        {
                            column.Item().BorderTop(1).BorderColor(DarkGray).PaddingTop(10);

                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"Generated on {DateTime.Now:MMM dd, yyyy 'at' hh:mm tt}")
                                    .FontSize(8)
                                    .FontColor(DarkGray);

                                row.RelativeItem().AlignCenter().Text("Thank you for your business!")
                                    .FontSize(9)
                                    .FontColor(PrimaryColor)
                                    .Italic();

                                row.RelativeItem().AlignRight().Text(text =>
                                {
                                    text.Span("Page ").FontSize(8).FontColor(DarkGray);
                                    text.CurrentPageNumber().FontSize(8).FontColor(DarkGray);
                                    text.Span(" of ").FontSize(8).FontColor(DarkGray);
                                    text.TotalPages().FontSize(8).FontColor(DarkGray);
                                });
                            });
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
