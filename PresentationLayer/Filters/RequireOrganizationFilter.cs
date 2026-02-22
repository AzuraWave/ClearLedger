using InfrastructureLayer.Identity.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PresentationLayer.Filters
{
    public class RequireOrganizationAttribute : TypeFilterAttribute
    {
        public RequireOrganizationAttribute() : base(typeof(RequireOrganizationFilter)) { }
    }

    public class RequireOrganizationFilter : IAsyncPageFilter
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RequireOrganizationFilter(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var user = await _userManager.GetUserAsync(context.HttpContext.User);
            if (user != null && await _userManager.IsInRoleAsync(user, "OrgUser") && user.OrganizationId == null)
            {
                // Allow the create page itself to run (avoid redirect loop)
                var viewEnginePath = context.ActionDescriptor.ViewEnginePath?.TrimEnd('/') ?? string.Empty;
                if (!string.Equals(viewEnginePath, "/OrganizationPages/Create", StringComparison.OrdinalIgnoreCase))
                {
                    context.Result = new RedirectToPageResult("/OrganizationPages/Create");
                    return;
                }
            }

            await next();
        }
        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        { 
            return Task.CompletedTask;
        }
    
    }
}
