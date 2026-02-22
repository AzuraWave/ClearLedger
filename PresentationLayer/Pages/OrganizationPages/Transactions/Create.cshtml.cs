using ApplicationLayer.DTOs.Transactions.Adjustment;
using ApplicationLayer.DTOs.Transactions.Discount;
using ApplicationLayer.DTOs.Transactions.Invoices;
using ApplicationLayer.DTOs.Transactions.Payments;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace PresentationLayer.Pages.OrganizationPages.Transactions
{
    public class CreateModel : PageModel
    {
        private readonly ITransactionService _transactionService;
        private readonly IProjectService _projectService;

        public CreateModel( ITransactionService transactionService,IProjectService projectService )
        {
            _transactionService = transactionService;
            _projectService = projectService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid ProjectId { get; set; }

        [BindProperty]
        public TransactionType Type { get; set; }

        [BindProperty]
        public decimal Amount { get; set; }

        [BindProperty]
        public DateTime Date { get; set; } = DateTime.Today;

        [BindProperty]
        public string? Reference { get; set; }

        [BindProperty]
        public bool IsPositive { get; set; } = true;

        [BindProperty]
        public List<CreateInvoiceLineDto> InvoiceLines { get; set; } = new List<CreateInvoiceLineDto>();

        public SelectList TransactionTypes =>
            new(Enum.GetValues<TransactionType>());

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var orgIdString = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdString))
            {
                ModelState.AddModelError(string.Empty, "OrganizationId claim is missing.");
                return Page();
            }
            var orgId = Guid.Parse(orgIdString);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                ModelState.AddModelError(string.Empty, "User identifier claim is missing.");
                return Page();
            }
            var userId = Guid.Parse(userIdString);
            

            var project = await _projectService.GetProjectAsync(ProjectId);
            if (project == null)
            {
                ModelState.AddModelError(string.Empty, "Project not found.");
                return Page();
            }

            var clientId = project.clientId;

            switch (Type)
            {
                case TransactionType.Invoice:

                    if (!InvoiceLines.Any())
                    {
                        ModelState.AddModelError(string.Empty, "At least one invoice line is required.");
                        return Page();
                    }

                    await _transactionService.CreateInvoiceAsync(
                        new CreateInvoiceDto
                        {
                            ProjectId = ProjectId,
                            ClientId = clientId,
                            Amount = Amount,
                            Date = Date,
                            Reference = Reference,
                            Lines = InvoiceLines
                            
                        },
                        orgId,
                        userId);
                    break;

                case TransactionType.Payment:
                    await _transactionService.CreateProjectPaymentAsync(
                        new CreateProjectPaymentDto
                        {
                            ProjectId = ProjectId,          
                            Amount = Amount,
                            Date = Date,
                            Reference = Reference
                        },
                        orgId,
                        userId);
                    break;

                case TransactionType.Discount:
                    var balance = project.Balance;
                    if (Amount > balance)
                    {
                        ModelState.AddModelError(string.Empty, "Discount exceeds current project balance.");
                        return Page(); 
                    }
                    if (Reference == null)
                    {
                        ModelState.AddModelError(string.Empty, "Reference is required for discounts.");
                        return Page();
                    }


                    await _transactionService.ApplyDiscountAsync(
                        new CreateDiscountDto
                        {
                            ProjectId = ProjectId,
                            ClientId = clientId,
                            Amount = Amount,
                            Date = Date,
                            Reason = Reference ?? "Discount"
                        },
                        orgId,
                        userId);
                    break;

                case TransactionType.Adjustment:

                    if (Reference == null)
                    {
                        ModelState.AddModelError(string.Empty, "Reference is required for Adjustments");
                        return Page();
                    }

                    await _transactionService.CreateAdjustmentAsync(
                        new CreateAdjustmentDto
                        {
                            ProjectId = ProjectId,
                            ClientId = clientId,
                            Amount = Amount,
                            Date = Date,
                            Reason = Reference ?? "Adjustment",
                            IsPositive = IsPositive
                        },
                        orgId,
                        userId);
                    break;
            }

            return RedirectToPage("/OrganizationPages/Projects/Details", new { id = ProjectId });
        }
    }
}
