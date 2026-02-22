using ApplicationLayer.DTOs.Client;
using ApplicationLayer.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer.Pages.OrganizationPages.Clients
{
    public class DeleteModel : PageModel
    {
        private readonly IClientService _clientService;

        public DeleteModel(IClientService clientService)
        {
            _clientService = clientService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public ClientReadDto? Client { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var orgId = Guid.Parse(User.FindFirst("OrganizationId")!.Value);
            Client = await _clientService.GetClientAsync(Id, orgId);

            if (Client == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var orgId = Guid.Parse(User.FindFirst("OrganizationId")!.Value);

            await _clientService.ArchiveClientAsync(Id, orgId);
            return RedirectToPage("./Index");
        }
    }
}
