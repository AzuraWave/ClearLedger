// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using InfrastructureLayer.Identity.User;
using System.Security.Claims;

namespace PresentationLayer.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            _logger.LogInformation("Login page requested. Authenticated: {IsAuthenticated}",
    User.Identity?.IsAuthenticated);
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/OrganizationPages/Dashboard/Index");
            }

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            _logger.LogInformation("Login attempt for Email {Email}", Input.Email);
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Login successful for Email {Email}", Input.Email);
                    var user = await _userManager.FindByEmailAsync(Input.Email);

                    await SynchronizeUserClaimsAsync(user);

                    if (await _userManager.IsInRoleAsync(user, "OrgUser"))
                    {
                        _logger.LogInformation("User {Email} logged in as OrgUser", Input.Email);
                     
                            if (user.OrganizationId == null)
                            {
                                // Redirect to org creation page if they have no organization yet
                                return RedirectToPage("/OrganizationPages/Create/Index");
                            }
           
                        

                        return RedirectToPage("/OrganizationPages/Dashboard/Index");

                    }
                    else if (await _userManager.IsInRoleAsync(user, "Customer"))
                    {
                        _logger.LogInformation("User {Email} logged in as Customer", Input.Email);
                       
                        return LocalRedirect("/Customer/Overview");
                    }
                    else
                        return LocalRedirect("/Index");
                }
                if (result.RequiresTwoFactor)
                {
                    _logger.LogInformation("Login requires 2FA for Email {Email}", Input.Email);
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    _logger.LogWarning("Invalid login attempt for Email {Email}", Input.Email);
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private async Task SynchronizeUserClaimsAsync(ApplicationUser user)
        {
            var claims = await _userManager.GetClaimsAsync(user);
            var claimsAdded = false;

            // Sync OrganizationId claim for OrgUser and Customer roles
            if (user.OrganizationId.HasValue && !claims.Any(c => c.Type == "OrganizationId"))
            {
                var addClaimResult = await _userManager.AddClaimAsync(user, new Claim("OrganizationId", user.OrganizationId.ToString()));
                if (addClaimResult.Succeeded)
                {
                    _logger.LogInformation("Added missing OrganizationId claim for user {Email}", user.Email);
                    claimsAdded = true;
                }
                else
                {
                    _logger.LogError("Failed to add OrganizationId claim for user {Email}: {Errors}",
                        user.Email, string.Join(", ", addClaimResult.Errors.Select(e => e.Description)));
                }
            }

            // Sync ClientId claim for Customer role
            if (user.ClientId.HasValue && !claims.Any(c => c.Type == "ClientId"))
            {
                var addClaimResult = await _userManager.AddClaimAsync(user, new Claim("ClientId", user.ClientId.ToString()));
                if (addClaimResult.Succeeded)
                {
                    _logger.LogInformation("Added missing ClientId claim for user {Email}", user.Email);
                    claimsAdded = true;
                }
                else
                {
                    _logger.LogError("Failed to add ClientId claim for user {Email}: {Errors}",
                        user.Email, string.Join(", ", addClaimResult.Errors.Select(e => e.Description)));
                }


            }

            // Refresh sign-in if any claims were added
            if (claimsAdded)
            {
                await _signInManager.RefreshSignInAsync(user);
                _logger.LogInformation("Refreshed sign-in for user {Email} after adding claims", user.Email);
            }
        }
    }
}
