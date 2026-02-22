using ApplicationLayer.DTOs.Client;
using ApplicationLayer.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace PresentationLayer.Pages.OrganizationPages.Clients
{
    public class EditModel : PageModel
    {
        private readonly IClientService _clientService;

        public EditModel(IClientService clientService)
        {
            _clientService = clientService;
        }

        public ClientReadDto? Client { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public ClientInputModel? Input { get; set; }
        public class ClientInputModel
        {
            [Required]
            public required string Name { get; set; }
            [Phone]
            [StringLength(20)]
            public string? PhoneNumber { get; set; }

            public string? BillingEmail { get; set; }
            public string? Address { get; set; }
            public string? Notes { get; set; }
        }

        public async Task OnGetAsync()
        {
            var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new Exception("OrgId missing"));
            var clientId = Id;
            Client = await _clientService.GetClientAsync(clientId, orgId);
            if (Client != null)
            {
                Input = new ClientInputModel
                {
                    Name = Client.Name,
                    PhoneNumber = Client.PhoneNumber,
                    BillingEmail = Client.BillingEmail,
                    Address = Client.Address,
                    Notes = Client.Notes
                };
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new Exception("OrgId missing"));
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("UserId missing"));
            var clientId = Id;
            if (Input == null)
            {
                return Page();
            }
            ClientUpdateDto clientUpdateDto = new ClientUpdateDto
            {
                Id = clientId,
                Name = Input.Name,
                PhoneNumber = Input.PhoneNumber,
                BillingEmail = Input.BillingEmail,
                Address = Input.Address,
                Notes = Input.Notes
            };
            await _clientService.UpdateClientAsync(orgId, clientUpdateDto, userId);
            return RedirectToPage("./Index");

        }
    }
}
