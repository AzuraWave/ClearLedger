using System;
using System.Linq;
using System.Threading.Tasks;
using ApplicationLayer.DTOs.Query;
using ApplicationLayer.DTOs.Transactions;
using ApplicationLayer.DTOs.Transactions.Query;
using ApplicationLayer.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer.Pages.OrganizationPages.Dashboard
{
    [Authorize(Roles = "OrgUser")]
    public class IndexModel : PageModel
    {
        private readonly IClientService _clientService;
        private readonly IProjectService _projectService;
        private readonly IOrganizationService _organizationService;
        private readonly ITransactionService _transactionService;

        public IndexModel(
            IClientService clientService,
            IProjectService projectService,
            ITransactionService transactionService,
            IOrganizationService organizationService)
        {
            _clientService = clientService;
            _projectService = projectService;
            _transactionService = transactionService;
            _organizationService = organizationService;
        }

        public int TotalClients { get; set; }
        public int TotalProjects { get; set; }
        public decimal TotalBalance { get; set; }

        public decimal TotalInvoices { get; set; }

        // Initialize with an empty PagedResult so the property is never null.
        public PagedResult<TransactionDto> RecentTransactions { get; set; } =
            new PagedResult<TransactionDto>(Enumerable.Empty<TransactionDto>(), 0);

        public async Task OnGetAsync()
        {
            var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new Exception("OrgId missing"));



            TotalClients = await _clientService.GetClientsTotal(orgId);
            TotalProjects = await _projectService.GetProjectsTotal(orgId);
            TotalBalance = await _organizationService.GetOrganizationBalanceAsync(orgId);
            TotalInvoices = await _transactionService.GetOrgInvoicesForCurrentMonth(orgId);

            var search = new TransactionQueryDto
            {
                OrganizationId = orgId,
                Page = 1,
                PageSize = 5,
                SortBy = "DateDesc"
            };

            RecentTransactions = await _transactionService.SearchTransactionsAsync(search);
        }
    }
}
