using ApplicationLayer.DTOs.Transactions.Invoices;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Entities;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [ApiController]
    [Route("api/v1/invoices")]
    public class InvoicesController : Controller
    {
        private readonly ITransactionService _invoiceService;

        public InvoicesController(ITransactionService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceDto dto)
        {
            var org = HttpContext.Items["Organization"] as Organization;
            if (org == null)
            {
                return Unauthorized();
            }


            var createdByUserId = org.DefaultAutomationUserId ?? org.CreatedBy;

            var invoiceId = await _invoiceService.CreateInvoiceAsync(dto, org.Id, createdByUserId);
            return Ok(new { InvoiceId = invoiceId });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoice(Guid id)
        {


            var org = HttpContext.Items["Organization"] as Organization;
            if (org == null)
            {
                return Unauthorized();
            }

            var invoice = await _invoiceService.GetInvoiceDetailsAsync(id, org.Id);
            if (invoice == null) return NotFound();
            return Ok(invoice);
        }

    }
}
