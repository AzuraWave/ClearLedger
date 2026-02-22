using ApplicationLayer.DTOs.Transactions;
using ApplicationLayer.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace PresentationLayer.Pages.OrganizationPages.Clients
{
    public class StatementModel : PageModel
    {

        private readonly ITransactionService _transactionService;
        private readonly IStatementDocumentGenerator _statementDocumentGenerator;
        private readonly IStatementExcelGenerator _statementExcelGenerator;

        public StatementModel(ITransactionService transactionService, IStatementDocumentGenerator statementDocumentGenerator, IStatementExcelGenerator statementExcelGenerator)
        {
            _transactionService = transactionService;
            _statementDocumentGenerator = statementDocumentGenerator;
            _statementExcelGenerator = statementExcelGenerator;
        }

        [BindProperty(SupportsGet = true)]
        public Guid ClientId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? From { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? To { get; set; }

        public StatementOfAccountDto? Statement { get; set; }
        public async Task OnGetAsync()
        {
            var orgId = Guid.Parse(User.FindFirst("OrganizationId")!.Value);

            if (From != null && To != null)
            {
                Statement = await _transactionService
                .GetStatementAsync(orgId, ClientId, From.Value, To.Value);
            }
        }

        public async Task<IActionResult> OnGetPrintPdfAsync(Guid clientId, DateTime from, DateTime to)
        {
            var orgIdClaim = User.FindFirstValue("OrganizationId");
            if (!Guid.TryParse(orgIdClaim, out var organizationId))
                return Forbid();


            var statement = await _transactionService.GetStatementAsync(organizationId, clientId, from, to);
            if (statement == null)
                return NotFound();

            var pdfBytes = _statementDocumentGenerator.Generate(statement);

            return File(pdfBytes, "application/pdf", $"Statement-{statement.ClientName}-{from:yyyyMMdd}-{to:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> OnGetExportExcelAsync(Guid clientId, DateTime from, DateTime to)
        {
            var orgIdClaim = User.FindFirstValue("OrganizationId");
            if (!Guid.TryParse(orgIdClaim, out var orgId))
                return Forbid();

            var statement = await _transactionService.GetStatementAsync(orgId, clientId, from, to);
            if (statement == null)
                return NotFound();

            var excelBytes = _statementExcelGenerator.Generate(statement);

            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Statement-{statement.ClientName}-{from:yyyyMMdd}-{to:yyyyMMdd}.xlsx"
            );
        }
    }
}
