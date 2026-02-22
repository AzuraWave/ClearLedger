using ApplicationLayer.DTOs.Projects;
using ApplicationLayer.DTOs.Query;
using ApplicationLayer.DTOs.Transactions;
using ApplicationLayer.DTOs.Transactions.Query;
using ApplicationLayer.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace PresentationLayer.Pages.Customer.Projects
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
        public ProjectReadDto? Project { get; set; }
        public PagedResult<ApplicationLayer.DTOs.Transactions.TransactionDto> Transactions { get; set; } = new PagedResult<ApplicationLayer.DTOs.Transactions.TransactionDto>(items: [], 0);

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

            var clientIdClaim = User.FindFirstValue("ClientId");
            if (!Guid.TryParse(clientIdClaim, out var clientId))
                return Forbid();


            // Build query DTO for service
            var query = new TransactionQueryDto
            {
                ProjectId = Id,
                OrganizationId = organizationId,
                ClientId = clientId,
                Search = SearchQuery,
                SortBy = SortBy,
                From = FromDate,
                To = ToDate,
                Page = PageNumber,
                PageSize = PageSize,
                MinAmount = MinAmount,
                MaxAmount = MaxAmount,
            };


            Transactions = await _transactionService.SearchTransactionsAsync(query);


            return Page();
        }
    }
}
