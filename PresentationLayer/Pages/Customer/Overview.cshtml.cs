using ApplicationLayer.DTOs.Query;
using ApplicationLayer.DTOs.Transactions;
using ApplicationLayer.DTOs.Transactions.Query;
using ApplicationLayer.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer.Pages.Customer
{
    [Authorize(Roles = "Customer")]
    public class OverviewModel : PageModel
    {
        private readonly IClientService _clientService;
        private readonly IProjectService _projectService;
        private readonly IOrganizationService _organizationService;
        private readonly ITransactionService _transactionService;

        public OverviewModel(
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


        public string ClientName { get; set; } = "Example Client";

        public decimal CurrentBalance { get; set; } = 1250.00m;
        public decimal TotalPaid { get; set; } = 5000.00m;
        public decimal TotalInvoiced { get; set; } = 6000.00m;
        public int TotalProjects { get; set; } = 0;

        public PagedResult<TransactionDto> RecentTransactions { get; set; } =
            new PagedResult<TransactionDto>(Enumerable.Empty<TransactionDto>(), 0);

        public async Task OnGetAsync()
        {
            try
            {
                var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new Exception("OrgId missing"));
                var clientId = Guid.Parse(User.FindFirst("ClientId")?.Value ?? throw new Exception("ClientId missing"));

                var client = await _clientService.GetClientAsync(clientId, orgId, false);

                var info = await _transactionService.GetClientOverviewAsync(orgId, clientId);

                if (client == null)
                {
                    throw new Exception("Client not found");
                }
                ClientName = client.Name;
                TotalPaid = info.TotalPaid;
                TotalInvoiced = info.TotalInvoiced;
                TotalProjects = info.ActiveProjects;
                CurrentBalance = client.Balance;



                var search = new TransactionQueryDto
                {
                    OrganizationId = orgId,
                    ClientId = clientId,
                    Page = 1,
                    PageSize = 5,
                    SortBy = "DateDesc"
                };

                RecentTransactions = await _transactionService.SearchTransactionsAsync(search);
            }
            catch (Exception ex) { 
                CurrentBalance = 0;
            }

        }
    }

}

