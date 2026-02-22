using ApplicationLayer.DTOs.Transactions.Payments;
using ApplicationLayer.Interfaces.Services;
using ApplicationLayer.Services;
using Azure;
using DomainLayer.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PresentationLayer.Pages.OrganizationPages.Transactions.Payments
{
    public class CreateModel : PageModel
    {
        private readonly IProjectService _projectService;
        private readonly ITransactionService _paymentService;

        public CreateModel(IProjectService projectService, ITransactionService paymentService)
        {
            _projectService = projectService;
            _paymentService = paymentService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid clientId { get; set; }

        [BindProperty]
        public decimal TotalAmount { get; set; }

        [BindProperty]
        public DateTime Date { get; set; } = DateTime.Today;

        [BindProperty]
        public string? Reference { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }
        [BindProperty(SupportsGet = true)]
        public ProjectStatus? Status { get; set; }

        [BindProperty]
        public List<PaymentAllocationDto> Allocations { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new Exception("OrgId missing"));
            // Load active projects for this client
            var projects = await _projectService.GetProjectsByClientAsync(clientId, orgId);

            if (!string.IsNullOrWhiteSpace(Search))
                projects = projects
                    .Where(p => p.Name.Contains(Search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (Status.HasValue)
                projects = projects.Where(p => p.ProjectStatus == Status).ToList();


            Allocations = projects.Select(p => new PaymentAllocationDto
            {
                ProjectId = p.Id,
                Name = p.Name,
                status = p.ProjectStatus,
                Amount = 0,
                Id = Guid.NewGuid()
            }).ToList();

            return Page();
        }

public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new Exception("OrgId missing"));
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("UserId missing"));

            var allocationsBalance = 0m;

            for (int i = 0; i < Allocations.Count; i++)
            {
                var alloc = Allocations[i];
                if (alloc.Amount < 0)
                {
                    ModelState.AddModelError($"Allocations[{i}].Amount", "Amount cannot be negative.");
                }
                allocationsBalance += alloc.Amount;
            }

            if (TotalAmount != allocationsBalance)
            {
                ModelState.AddModelError(nameof(TotalAmount), "Total amount must equal the sum of allocations.");
            }

            if (!ModelState.IsValid)
                return Page();

            var dto = new CreateClientPaymentDto
            {
                ClientId = clientId,
                TotalAmount = TotalAmount,
                Date = Date,
                Reference = Reference,
                Allocations = Allocations
            };

            try
            {
                await _paymentService.CreateClientPaymentAsync(dto, orgId, userId);
                return RedirectToPage("/OrganizationPages/Clients/Details", new { id = clientId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }
    }
}
