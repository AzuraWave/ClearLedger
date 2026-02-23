using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ApplicationLayer.DTOs.Query;
using ApplicationLayer.DTOs.Transactions;
using ApplicationLayer.DTOs.Transactions.Query;
using ApplicationLayer.Interfaces.Services;
using InfrastructureLayer.Identity.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            IClientService clientService,
            IProjectService projectService,
            ITransactionService transactionService,
            IOrganizationService organizationService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _clientService = clientService;
            _projectService = projectService;
            _transactionService = transactionService;
            _organizationService = organizationService;
            _userManager = userManager;
            _signInManager = signInManager;
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
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            
            // If the claim is missing, try to get it from the database and refresh the sign-in
            if (string.IsNullOrEmpty(orgIdClaim))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.OrganizationId == null)
                {
                    // User has no organization, redirect to create page
                    Response.Redirect("/OrganizationPages/Create");
                    return;
                }

                // Refresh the sign-in to update the authentication cookie with claims
                await _signInManager.RefreshSignInAsync(user);
                
                // Use the organization ID from the database
                orgIdClaim = user.OrganizationId.ToString();
            }

            var orgId = Guid.Parse(orgIdClaim);

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
