using InfrastructureLayer.Context;
using InfrastructureLayer.Identity.Roles;
using InfrastructureLayer.Identity.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PresentationLayer.Middleware;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace PresentationLayerTest
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>  
    {
        
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");

            base.ConfigureWebHost(builder);


            builder.ConfigureTestServices(services =>
            {

                services.RemoveAll(typeof(DbContextOptions<LedgerDbContext>));

                string connectionString = "Server=(localdb)\\mssqllocaldb;Database=LedgerTestDb;Trusted_Connection=True;MultipleActiveResultSets=true";

                services.AddDbContext<LedgerDbContext>(options =>
                {
                    options.UseSqlServer(connectionString);
                });

                services.AddIdentity<ApplicationUser, Roles>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                }).AddEntityFrameworkStores<LedgerDbContext>()
        .AddDefaultTokenProviders();

                services.AddSingleton<IEmailSender, NoOpEmailSender>();


                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<LedgerDbContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                Seeding.InitializeTestDB(db);
            });

           


        }
    }

    
}
