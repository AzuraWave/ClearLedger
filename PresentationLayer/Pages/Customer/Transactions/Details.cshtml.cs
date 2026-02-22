using ApplicationLayer.DTOs.Client;
using ApplicationLayer.DTOs.Organization;
using ApplicationLayer.DTOs.Transactions;
using ApplicationLayer.DTOs.Transactions.Invoices;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Entities;
using InfrastructureLayer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace PresentationLayer.Pages.Customer.Transactions
{
    public class DetailsModel : PageModel
    {
        private readonly ITransactionService _service;
        private readonly IClientService _clientService;
        private readonly IOrganizationService _organizationService;
        private readonly IInvoiceDocumentGenerator _invoiceDocumentGenerator;
        private readonly IInvoiceExcelGenerator _invoiceExcelGenerator;


        public DetailsModel(ITransactionService service, IClientService clientService, IOrganizationService organizationService
            , IInvoiceDocumentGenerator invoiceDocumentGenerator, IInvoiceExcelGenerator invoiceExcelGenerator)
        {
            _service = service;
            _clientService = clientService;
            _organizationService = organizationService;
            _invoiceDocumentGenerator = invoiceDocumentGenerator;
            _invoiceExcelGenerator = invoiceExcelGenerator;
        }

        [BindProperty(SupportsGet = true)]
        public Guid invoiceId { get; set; }



        public InvoiceReadDto? Invoice { get; set; }
        public ClientReadDto? Client { get; set; }
        public OrganizationReadDto? Organization { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            var orgIdClaim = User.FindFirstValue("OrganizationId");
            if (!Guid.TryParse(orgIdClaim, out var organizationId))
                return Forbid();

            var clientIdClaim = User.FindFirstValue("ClientId");
            if (!Guid.TryParse(clientIdClaim, out var clientId))
                return Forbid();
            try
            {
                Invoice = await _service.GetInvoiceDetailsAsync(invoiceId, organizationId);
                if (Invoice == null)
                    return NotFound();
                Client = await _clientService.GetClientAsync(clientId, organizationId);
                if (Client == null)
                {
                    return NotFound();
                }

                Organization = await _organizationService.GetOrganizationAsync(organizationId);
                if (Organization == null)
                {
                    return NotFound();
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }

            return Page();

        }

        public async Task<IActionResult> OnGetPrintAsync(Guid id)
        {
            var orgIdClaim = User.FindFirstValue("OrganizationId");
            if (!Guid.TryParse(orgIdClaim, out var organizationId))
                return Forbid();

            var invoice = await _service.GetInvoiceDetailsAsync(id, organizationId);
            if (invoice == null)
                return NotFound();

            var clientIdClaim = User.FindFirstValue("ClientId");
            if (!Guid.TryParse(clientIdClaim, out var clientId))
                return Forbid();

            
            var client = await _clientService.GetClientAsync(clientId, organizationId);
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

            var invoice = await _service.GetInvoiceDetailsAsync(id, organizationId);
            if (invoice == null)
                return NotFound();

            var clientIdClaim = User.FindFirstValue("ClientId");
            if (!Guid.TryParse(clientIdClaim, out var clientId))
                return Forbid();


            var client = await _clientService.GetClientAsync(clientId, organizationId);
            if (client == null)
                return NotFound();

            var excelBytes = _invoiceExcelGenerator.Generate(invoice, client);
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Invoice-{invoice.InvoiceNumber}.xlsx");
        }

    }
}
