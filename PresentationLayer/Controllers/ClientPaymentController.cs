using ApplicationLayer.DTOs.Transactions.Payments;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Entities;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [ApiController]
    [Route("api/v1/payments/client")]
    public class ClientPaymentController : Controller
    {
        private readonly ITransactionService _paymentService;

        public ClientPaymentController(ITransactionService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateClientPaymentDto dto)
        {
            var org = HttpContext.Items["Organization"] as Organization;
            if (org == null)
            {
                return Unauthorized();
            }
            var createduser = org.DefaultAutomationUserId ?? org.CreatedBy;
            var paymentId = await _paymentService.CreateClientPaymentAsync(dto, org.Id, createduser);
            return Ok(new { PaymentId = paymentId });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var org = HttpContext.Items["Organization"] as Organization;
            if (org == null)
            {
                return Unauthorized();
            }
            var payment = await _paymentService.GetClientPaymentDetailsAsync(id, org.Id);
            if (payment == null) return NotFound();
            return Ok(payment);
        }
    }
}
