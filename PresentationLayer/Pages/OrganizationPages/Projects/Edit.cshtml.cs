using ApplicationLayer.DTOs.Projects;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PresentationLayer.Pages.OrganizationPages.Projects
{
    public class EditModel : PageModel
    {
        private readonly IProjectService _projectService;

        public EditModel(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            public Guid Id { get; set; }

            [Required]
            public string Name { get; set; } = string.Empty;

            public string? Description { get; set; }

            public ProjectStatus? ProjectStatus { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var project = await _projectService.GetProjectAsync(Id);
            if (project == null)
                return NotFound();

            Input.Id = project.Id;
            Input.Name = project.Name;
            Input.Description = project.Description;
            Input.ProjectStatus = project.ProjectStatus;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(idClaim, out var userId))
                return Forbid();

            var dto = new ProjectUpdateDto
            {
                Id = Input.Id,
                Name = Input.Name,
                Description = Input.Description,
                ProjectStatus = Input.ProjectStatus
            };

            await _projectService.UpdateProjectAsync(dto, userId);

            return RedirectToPage("./Details", new { id = Input.Id });
        }
    }
}
