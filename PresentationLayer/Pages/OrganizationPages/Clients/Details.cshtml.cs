using ApplicationLayer.DTOs.Client;
using ApplicationLayer.DTOs.Projects;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Entities;
using DomainLayer.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer.Pages.OrganizationPages.Clients
{
    public class DetailsModel : PageModel
    {
        private readonly IClientService _clientService;
        private readonly IProjectService _projectService;
        private readonly IBalanceService _balanceService;
        public DetailsModel(IClientService clientService, IProjectService projectService, IBalanceService balanceService)
        {
            _clientService = clientService;
            _projectService = projectService;
            _balanceService = balanceService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }
        public ClientReadDto? Client { get; set; }

        public IEnumerable<ProjectReadDto> Projects { get; set; } = new List<ProjectReadDto>();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }
        [BindProperty(SupportsGet = true)]
        public ProjectStatus? Status { get; set; }



        public async Task OnGetAsync()
        {
            var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new Exception("OrgId missing"));
            Client = await _clientService.GetClientAsync(Id, orgId);
            if (Client == null) NotFound();
            var projects = await _projectService.GetProjectsByClientAsync(Id, orgId, false);

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

        public async Task<IActionResult> OnPostArchiveProjectAsync(Guid projectId)
        {
            // Get current user id
            var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(idClaim, out var userId))
                return Forbid();

            
            await _projectService.ArchiveProjectAsync(projectId, userId);

            // Redirect back to this client details page
             return RedirectToPage(new { id = this.Id });
        }

        public async Task<IActionResult> OnPostRecalculateBalanceAsync(Guid clientId)
        {
            var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new Exception("OrgId missing"));

            await _balanceService.RecalculateClientAsync(clientId, orgId);
               

            return RedirectToPage(new { id = clientId });
        }


    }
}
