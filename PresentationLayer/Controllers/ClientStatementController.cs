using ApplicationLayer.Interfaces.Services;
using DomainLayer.Entities;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [ApiController]
    [Route("api/v1/statements/client")]
    public class ClientStatementController : Controller
    {

        private readonly ITransactionService _transactionService;

        public ClientStatementController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet("{clientId}")]
        public async Task<IActionResult> GetClientStatement(Guid clientId, [FromQuery] DateTime from ,
            [FromQuery] DateTime to )
        {
            var org = HttpContext.Items["Organization"] as Organization;
            if (org == null) return NotFound("Organization not found");

            var statement = await _transactionService.GetStatementAsync(org.Id, clientId, from, to);
            if (statement == null) return NotFound("No statement found for this client");

            return Ok(statement);
        }
    }
}
