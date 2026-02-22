using ApplicationLayer.DTOs.Projects;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PresentationLayer.Pages.OrganizationPages.Projects
{
    public class CreateModel : PageModel
    {
        private readonly IProjectService _projectService;
        private readonly IClientService _clientService;

        public CreateModel(IProjectService projectService, IClientService clientService)
        {
            _projectService = projectService;
            _clientService = clientService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            public string Name { get; set; } = string.Empty;

            public string? Description { get; set; }

            [Required]
            public Guid ClientId { get; set; }

            [Required]
            public Guid OrganizationId { get; set; }
        }

        public void OnGet(Guid clientId, Guid organizationId)
        {
            Input.ClientId = clientId;
            Input.OrganizationId = organizationId;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // get current user id from claims (Identity uses NameIdentifier = Guid)
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(idClaim, out var userId))
            {
                return Forbid();
            }

            // Validate that the client exists and belongs to the given organization.
            var client = await _clientService.GetClientAsync(Input.ClientId, Input.OrganizationId);
            if (client == null)
            {
                ModelState.AddModelError(string.Empty, "Client not found for the selected organization.");
                return Page();
            }

            var dto = new ProjectCreateDto
            {
                Name = Input.Name,
                Description = Input.Description,
                clientId = Input.ClientId,
                projectStatus = ProjectStatus.Active,
                organizationId = Input.OrganizationId
            };

            var projectId = await _projectService.CreateProjectAsync(dto, userId);
            return RedirectToPage("/OrganizationPages/Projects/Details", new { id = projectId });
        }
    }
}
