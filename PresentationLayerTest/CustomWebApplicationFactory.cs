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
        private bool _isInitialized = false;
        private readonly object _lock = new object();
        private DbConnection _connection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");

            base.ConfigureWebHost(builder);


            builder.ConfigureTestServices(services =>
            {

                services.RemoveAll(typeof(DbContextOptions<LedgerDbContext>));

                // Use SQLite in-memory database for tests
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                services.AddDbContext<LedgerDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });

                services.AddIdentity<ApplicationUser, Roles>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                }).AddEntityFrameworkStores<LedgerDbContext>()
        .AddDefaultTokenProviders();

                services.AddSingleton<IEmailSender, NoOpEmailSender>();

                // Initialize the database only once
                if (!_isInitialized)
                {
                    lock (_lock)
                    {
                        if (!_isInitialized)
                        {
                            var sp = services.BuildServiceProvider();
                            using var scope = sp.CreateScope();
                            var db = scope.ServiceProvider.GetRequiredService<LedgerDbContext>();
                           try
                           {
                               // Create the schema for the in-memory database
                               db.Database.EnsureCreated();
                               Seeding.InitializeTestDB(db);
                               _isInitialized = true;
                           }
                           catch
                           {
                               // If initialization fails, ensure we can retry
                               _isInitialized = false;
                               throw;
                           }
                        }
                    }
                }
            });




        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    _connection?.Close();
                    _connection?.Dispose();
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }
            base.Dispose(disposing);
        }
    }

    
}
