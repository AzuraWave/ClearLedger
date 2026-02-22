using ApplicationLayer.Interfaces.Services;
using InfrastructureLayer.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Cryptography;
using System.Text;

namespace PresentationLayer.Pages.OrganizationPages.ApiKey
{
    public class IndexModel : PageModel
    {

        private readonly IOrganizationService _organizationService;

        public IndexModel(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        public Guid OrganizationId { get; set; }

        public string? NewApiKey { get; set; }

        public void OnGet(Guid orgId)
        {
            OrganizationId = orgId;
        }
        public async Task<IActionResult> OnPostGenerateAsync()
        {
            var orgId = Guid.Parse(User.FindFirst("OrganizationId")!.Value);
            OrganizationId = orgId;
            var ApiKey = await _organizationService.GenerateApiKeyAsync(OrganizationId);

            NewApiKey = ApiKey;

            return Page();
        }

    }

    
}