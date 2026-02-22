using InfrastructureLayer.Identity.Roles;
using Microsoft.AspNetCore.Identity;

namespace PresentationLayer.DbConfiguration
{
    public class DbInitializer
    {

        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Roles>>();
            string[] roles = ["OrgUser", "Customer"];
            foreach (var role in roles)
            {
                var roleExist = await roleManager.RoleExistsAsync(role);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new Roles { Name = role });
                }
            }
        }



    }
}
