using InfrastructureLayer.Identity.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer.Pages
{
    public class IndexModel : PageModel
    {

        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                // Not signed in
                return RedirectToPage("/Account/Login");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/Account/Login");

            if (await _userManager.IsInRoleAsync(user, "OrgUser"))
            {
                // If OrgUser has no organization, redirect to org creation
                if (user.OrganizationId == null)
                    return RedirectToPage("/OrganizationPages/Create");

                return LocalRedirect("/OrganizationPages/Dashboard");
            }
            else if (await _userManager.IsInRoleAsync(user, "Customer"))
            {
                return RedirectToPage("/Customer/Overview");
            }
            else
            {
                return Page();
            }
        }
    }
}
