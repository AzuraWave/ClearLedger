using InfrastructureLayer.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace PresentationLayerTest
{
    public class TestItemsStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.Use(async (context, nextMiddleware) =>
                {
                    // Debug: Check if header exists
                    if (context.Request.Headers.TryGetValue("X-Test-OrgId", out var orgIdStr))
                    {
                        System.Diagnostics.Debug.WriteLine($"Header found: {orgIdStr}");

                        if (Guid.TryParse(orgIdStr, out var orgId))
                        {
                            System.Diagnostics.Debug.WriteLine($"GUID parsed: {orgId}");

                            var db = context.RequestServices.GetRequiredService<LedgerDbContext>();
                            var org = await db.Organizations.FindAsync(orgId);

                            if (org == null)
                            {
                                System.Diagnostics.Debug.WriteLine($"Org NOT found in DB. Total orgs: {db.Organizations.Count()}");
                                throw new Exception($"Middleware: Org {orgId} not found in DB. Count: {db.Organizations.Count()}");
                            }

                            System.Diagnostics.Debug.WriteLine($"Org found: {org.Name}");
                            context.Items["Organization"] = org;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to parse GUID from: {orgIdStr}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("X-Test-OrgId header not found");
                    }

                    await nextMiddleware();
                });

                next(app);
            };
        }
    }
}
