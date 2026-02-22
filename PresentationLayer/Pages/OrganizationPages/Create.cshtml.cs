using ApplicationLayer.Interfaces.Services;
using DomainLayer.Entities;
using InfrastructureLayer.Context;
using InfrastructureLayer.Identity.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PresentationLayer.Pages.OrganizationPages
{
    [Authorize(Roles = "OrgUser")]
    public class CreateModel : PageModel
    {
        private readonly LedgerDbContext _context;
        private readonly IOrganizationService _organizationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public CreateModel(LedgerDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, 
            IOrganizationService organizationService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _organizationService = organizationService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [Required]
            public string Name { get; set; } = string.Empty;

        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"User not found");
            }
            if (user.OrganizationId != null)
            {
                // Redirect if org already exists
                return RedirectToPage("/OrganizationPages/Dashboard/Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"User not found");
            }
            if (!ModelState.IsValid) return Page();

            var org = await _organizationService.CreateOrganizationAsync(Input.Name, user.Id);
            user.OrganizationId = org.Id;
            await _userManager.UpdateAsync(user);

            // Persist OrganizationId as a claim and refresh sign-in so cookie contains it
            var claims = await _userManager.GetClaimsAsync(user);
            if (!claims.Any(c => c.Type == "OrganizationId"))
            {
                await _userManager.AddClaimAsync(user, new Claim("OrganizationId", org.Id.ToString()));
            }
            await _signInManager.RefreshSignInAsync(user);

            return RedirectToPage("/OrganizationPages/Dashboard/Index");
        }
    }
}
