using ApplicationLayer.DTOs.Client;
using ApplicationLayer.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PresentationLayer.Pages.OrganizationPages.Clients
{
    public class CreateModel : PageModel
    {
        private readonly IClientService _clientService;

        public CreateModel(IClientService clientService)
        {
            _clientService = clientService;
        }

        [BindProperty]
        public ClientInputModel Input { get; set; } = new ClientInputModel { Name = " " };

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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var orgId = Guid.Parse(User.FindFirst("OrganizationId")!.Value);
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            ClientCreateDto clientCreateDto = new ClientCreateDto
            {
                Name = Input.Name,
                PhoneNumber = Input.PhoneNumber,
                BillingEmail = Input.BillingEmail,
                Address = Input.Address,
                Notes = Input.Notes
            };

            await _clientService.CreateClientAsync(orgId, clientCreateDto, userId);

            return RedirectToPage("./Index");
        }
    }
}
