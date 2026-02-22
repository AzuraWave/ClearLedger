using ApplicationLayer.DTOs.Client;
using ApplicationLayer.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace PresentationLayer.Pages.OrganizationPages.Clients
{
    public class LinkUserModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IClientService _clientService;

        public LinkUserModel(
            IUserService userService,
            IClientService clientService)
        {
            _userService = userService;
            _clientService = clientService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid ClientId { get; set; }

        [BindProperty]
        public required InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public string FirstName { get; set; } = string.Empty;
            [Required]
            public string LastName { get; set; } = string.Empty;

            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public required string Password { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public required string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var orgId = Guid.Parse(User.FindFirst("OrganizationId")!.Value);

            var client = await _clientService.GetClientAsync(ClientId, orgId);
            if (client == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var orgId = Guid.Parse(User.FindFirst("OrganizationId")!.Value);

            var dto = new CreateClientUserDto
            {
                OrganizationId = orgId,
                ClientId = ClientId,
                Email = Input.Email,
                TemporaryPassword = Input.Password,
                Role = "Customer",
                FirstName = Input.FirstName,
                LastName = Input.LastName
            };
            try
            {
                await _userService.CreateClientUserAsync(dto);
                TempData["SuccessMessage"] = "User created successfully.";
                return RedirectToPage("./Details", new { id = ClientId });
            }
            catch (InvalidOperationException ex)
            {
                
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }
    }
}
