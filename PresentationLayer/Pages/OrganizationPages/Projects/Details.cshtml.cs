using ApplicationLayer.DTOs.Projects;
using ApplicationLayer.DTOs.Query;
using ApplicationLayer.DTOs.Transactions;
using ApplicationLayer.DTOs.Transactions.Query;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace PresentationLayer.Pages.OrganizationPages.Projects
{
    public class DetailsModel : PageModel
    {
        private readonly IProjectService _projectService;
        private readonly ITransactionService _transactionService;
        private readonly IBalanceService _balanceService;

        public DetailsModel(IProjectService projectService, ITransactionService transactionService, IBalanceService balanceService)
        {
            _projectService = projectService;
            _transactionService = transactionService;
            _balanceService = balanceService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public ProjectReadDto? Project { get; set; }
        public PagedResult<TransactionDto> Transactions { get; set; } = new PagedResult<TransactionDto>(items: [] , 0 );

        // Search
        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "DateDesc";
        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 5;

        [BindProperty(SupportsGet = true)]
        public decimal? MinAmount { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxAmount { get; set; }

        public int TotalPages => (int)Math.Ceiling((decimal)Transactions.TotalCount / PageSize);



        public async Task<IActionResult> OnGetAsync()
        {
            Project = await _projectService.GetProjectAsync(Id);
            if (Project == null)
                return NotFound();

            var orgIdClaim = User.FindFirstValue("OrganizationId");
            if (!Guid.TryParse(orgIdClaim, out var organizationId))
                return Forbid();

            // Build query DTO for service
            var query = new TransactionQueryDto
            {
                ProjectId = Id,
                OrganizationId = organizationId,
                Search = SearchQuery,
                SortBy = SortBy,
                From = FromDate,
                To = ToDate,
                Page = PageNumber,
                PageSize = PageSize,
                MinAmount = MinAmount,
                MaxAmount = MaxAmount,
            };

            // Fetch filtered, sorted, paged results
            Transactions = await _transactionService.SearchTransactionsAsync(query);

            return Page();
        }

        public async Task<IActionResult> OnPostArchiveAsync()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(idClaim, out var userId))
                return Forbid();

            await _projectService.ArchiveProjectAsync(Id, userId);

            if (!string.IsNullOrWhiteSpace(ReturnUrl))
                return LocalRedirect(ReturnUrl);

            return RedirectToPage("./Details", new { id = Id });
        }

        public async Task<IActionResult> OnPostVoidAsync(Guid entryId, TransactionType entryType)
        {
            if (entryType != TransactionType.Invoice && entryType != TransactionType.Payment)
                return BadRequest("Invalid transaction type.");

            var orgIdClaim = User.FindFirstValue("OrganizationId");
            if (!Guid.TryParse(orgIdClaim, out var organizationId))
                return Forbid();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Forbid();

            if (entryType == TransactionType.Invoice)
                await _transactionService.VoidInvoiceAsync(entryId, organizationId, userId);
            else if (entryType == TransactionType.Payment)
                await _transactionService.VoidPaymentAsync(entryId, organizationId, userId);

            return RedirectToPage(new
            {
                id = Id,
                SearchQuery,
                SortBy,
                FromDate,
                ToDate,
                PageNumber,
                PageSize,
                MinAmount,
                MaxAmount
            });
        }

        public async Task<IActionResult> OnPostRecalculateBalanceAsync(Guid projectId)
        {
            var orgId = Guid.Parse(User.FindFirstValue("OrganizationId") ?? throw new Exception("OrgId missing"));

            await _balanceService.RecalculateProjectAsync(projectId, orgId);


            return RedirectToPage(new
            {
                id = projectId,
                SearchQuery,
                SortBy,
                FromDate,
                ToDate,
                PageNumber,
                PageSize,
                MinAmount,
                MaxAmount
            });
        }
    }
}
