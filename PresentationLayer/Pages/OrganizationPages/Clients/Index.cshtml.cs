using ApplicationLayer.DTOs.Client;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Entities;
using DomainLayer.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer.Pages.OrganizationPages.Clients
{
    [Authorize(Roles = "OrgUser")]
    public class IndexModel : PageModel
    {
        private readonly IClientService _clientService;

        public IndexModel(IClientService clientService)
        {
            _clientService = clientService;
        }

        public IEnumerable<ClientReadDto> Clients { get; set; } = new List<ClientReadDto>();

        [BindProperty(SupportsGet = true)]
        public string searchTerm { get; set; }
        [BindProperty(SupportsGet = true)]
        public ClientStatus Status { get; set; }

        public async Task OnGetAsync(string searchTerm, string status)
        {
            var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new Exception("OrgId missing"));

            var clients = await _clientService.GetClientsByOrganizationAsync(orgId);

            // In-memory filtering by search term
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                clients = clients.Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // In-memory filtering by status
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<DomainLayer.Enums.ClientStatus>(status, out var parsedStatus))
                {
                    clients = clients.Where(c => c.Status == parsedStatus);
                }
            }

            // Assign filtered list
            Clients = clients.ToList();

        }
    }
}
