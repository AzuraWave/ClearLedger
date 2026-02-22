using ApplicationLayer.DTOs.Client;
using ApplicationLayer.DTOs.Projects;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Entities;
using DomainLayer.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer.Pages.Customer.Projects
{
    public class IndexModel : PageModel
    {
        private readonly IClientService _clientService;
        private readonly IProjectService _projectService;
        private readonly IBalanceService _balanceService;

        public IndexModel(IClientService clientService, IProjectService projectService, IBalanceService balanceService)
        {
            _clientService = clientService;
            _projectService = projectService;
            _balanceService = balanceService;
        }


        public IEnumerable<ProjectReadDto> Projects { get; set; } = new List<ProjectReadDto>();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }
        [BindProperty(SupportsGet = true)]
        public ProjectStatus? Status { get; set; }

        public async Task OnGetAsync()
        {
            var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new Exception("OrgId missing"));
            var clientId = Guid.Parse(User.FindFirst("ClientId")?.Value ?? throw new Exception("ClientId missing"));
            var projects = await _projectService.GetProjectsByClientAsync(clientId, orgId, true);

            if (!string.IsNullOrWhiteSpace(Search))
                projects = projects
                    .Where(p => p.Name.Contains(Search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (Status.HasValue)
                projects = projects
                    .Where(p => p.ProjectStatus == Status.Value)
                    .ToList();

            Projects = projects;
        }
    }
}
