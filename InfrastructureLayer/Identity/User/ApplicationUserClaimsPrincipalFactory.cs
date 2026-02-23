using System.Security.Claims;
using InfrastructureLayer.Identity.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace InfrastructureLayer.Identity.User
{
    /// <summary>
    /// Custom claims principal factory that automatically synchronizes claims from the ApplicationUser database properties.
    /// This ensures OrganizationId and ClientId claims are always present in the user's ClaimsPrincipal when they exist in the database.
    /// </summary>
    public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, Roles.Roles>
    {
        public ApplicationUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<Roles.Roles> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Add OrganizationId claim if the user has an organization
            if (user.OrganizationId.HasValue)
            {
                identity.AddClaim(new Claim("OrganizationId", user.OrganizationId.Value.ToString()));
            }

            // Add ClientId claim if the user has a client
            if (user.ClientId.HasValue)
            {
                identity.AddClaim(new Claim("ClientId", user.ClientId.Value.ToString()));
            }

            return identity;
        }
    }
}
