using ApplicationLayer.DTOs.Client;
using ApplicationLayer.DTOs.Organization;
using ApplicationLayer.DTOs.Transactions.Invoices;
using ApplicationLayer.Interfaces.Services;
using InfrastructureLayer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace PresentationLayer.Pages.OrganizationPages.Transactions.Invoices
{
    public class DetailsModel : PageModel
    {
        private readonly ITransactionService _transactionService;
        private readonly IClientService _clientService;
        private readonly IOrganizationService _organizationService;
        private readonly IInvoiceDocumentGenerator _invoiceDocumentGenerator;
        private readonly IInvoiceExcelGenerator _invoiceExcelGenerator;

        public DetailsModel(
            ITransactionService transactionService,
            IClientService clientService,
            IOrganizationService organizationService,
            IInvoiceDocumentGenerator invoiceDocumentGenerator,
            IInvoiceExcelGenerator invoiceExcelGenerator)
        {
            _transactionService = transactionService;
            _clientService = clientService;
            _organizationService = organizationService;
            _invoiceDocumentGenerator = invoiceDocumentGenerator;
            _invoiceExcelGenerator = invoiceExcelGenerator;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public InvoiceReadDto? Invoice { get; set; }
        public ClientReadDto? Client { get; set; }
        public OrganizationReadDto? Organization { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var orgIdClaim = User.FindFirstValue("OrganizationId");
            if (!Guid.TryParse(orgIdClaim, out var organizationId))
                return Forbid();

            try
            {
                Invoice = await _transactionService.GetInvoiceDetailsAsync(Id, organizationId);
                if (Invoice == null)
                    return NotFound();

                if (!Invoice.ClientId.HasValue)
                    return NotFound();

                Client = await _clientService.GetClientAsync(Invoice.ClientId.Value, organizationId);
                if (Client == null)
                    return NotFound();

                Organization = await _organizationService.GetOrganizationAsync(organizationId);
                if (Organization == null)
                    return NotFound();
            }
            catch (Exception ex)
            {
                // Log the exception here if you have logging configured
                return StatusCode(500, "An error occurred while processing your request.");
            }

            return Page();
        }

        public async Task<IActionResult> OnGetPrintAsync(Guid id)
        {
            var orgIdClaim = User.FindFirstValue("OrganizationId");
            if (!Guid.TryParse(orgIdClaim, out var organizationId))
                return Forbid();

            var invoice = await _transactionService.GetInvoiceDetailsAsync(id, organizationId);
            if (invoice == null)
                return NotFound();

            if (!invoice.ClientId.HasValue)
                return NotFound();

            var client = await _clientService.GetClientAsync(invoice.ClientId.Value, organizationId);
            if (client == null)
                return NotFound();

            var organization = await _organizationService.GetOrganizationAsync(organizationId);
            if (organization == null)
                return NotFound();

            var pdfBytes = _invoiceDocumentGenerator.Generate(invoice, client, organization);

            return File(pdfBytes, "application/pdf", $"Invoice-{invoice.InvoiceNumber}.pdf");
        }

        public async Task<IActionResult> OnGetExcelAsync(Guid id)
        {
            var orgIdClaim = User.FindFirstValue("OrganizationId");
            if (!Guid.TryParse(orgIdClaim, out var organizationId))
                return Forbid();

            var invoice = await _transactionService.GetInvoiceDetailsAsync(id, organizationId);
            if (invoice == null)
                return NotFound();

            if (!invoice.ClientId.HasValue)
                return NotFound();

            var client = await _clientService.GetClientAsync(invoice.ClientId.Value, organizationId);
            if (client == null)
                return NotFound();

            var excelBytes = _invoiceExcelGenerator.Generate(invoice, client);
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Invoice-{invoice.InvoiceNumber}.xlsx");
        }
    }
}

