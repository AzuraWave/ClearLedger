

using ApplicationLayer.Interfaces.Patterns;
using ApplicationLayer.Interfaces.Repositories;
using ApplicationLayer.Interfaces.Services;
using ApplicationLayer.Services;
using InfrastructureLayer.Context;
using InfrastructureLayer.Identity.Roles;
using InfrastructureLayer.Identity.User;
using InfrastructureLayer.Repositories;
using InfrastructureLayer.Repositories.Patterns;
using InfrastructureLayer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using PresentationLayer.DbConfiguration;
using PresentationLayer.Filters;
using PresentationLayer.Middleware;
using QuestPDF.Infrastructure;
using Serilog;
using Serilog.Filters;


var builder = WebApplication.CreateBuilder(args);

var isTest = builder.Environment.IsEnvironment("Test");

// Application Layer
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IBalanceService, BalanceService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

// Infrastructure Layer
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdjustmentRepository, AdjustmentRepository>();
builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IBalanceRepository, BalanceRepository>();
builder.Services.AddScoped<IInvoiceDocumentGenerator, InvoicePdfGenerator>();
builder.Services.AddScoped<IInvoiceExcelGenerator, InvoiceExcelGenerator>();
builder.Services.AddScoped<IStatementDocumentGenerator, StatementPdfGenerator>();
builder.Services.AddScoped<IStatementExcelGenerator, StatementExcelGenerator>();
if (!isTest)
{
    builder.Services.AddDbContext<LedgerDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddIdentity<ApplicationUser, Roles>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    }).AddEntityFrameworkStores<LedgerDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.AddSingleton<IEmailSender, NoOpEmailSender>();
}
//Presentation layer 

builder.Host.UseSerilog((context, services, configuration) => configuration
    .Enrich.FromLogContext()  
    .WriteTo.Console()
        // Request logs
        .WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(Matching.FromSource("Microsoft.AspNetCore.Hosting"))
            .WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), "Logs/requests-.json", rollingInterval: RollingInterval.Day)
            .WriteTo.Console())
        // Application logs
        .WriteTo.Logger(lc => lc
            .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Hosting"))
            .WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), "Logs/app-.json", rollingInterval: RollingInterval.Day)
            .WriteTo.Console())
    .ReadFrom.Configuration(context.Configuration)
);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OrgUserPolicy", policy =>
        policy.RequireRole("OrgUser"));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Register");

    // Apply RequireOrganization filter to all pages under /OrganizationPages
    options.Conventions.AddFolderApplicationModelConvention(
        "/OrganizationPages",
        model => model.Filters.Add(new RequireOrganizationAttribute()));

    options.Conventions.AuthorizeFolder("/OrganizationPages", "OrgUserPolicy");
});
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Automation API",
        Version = "v1"
    });

    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = "X-API-KEY",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Enter API Key"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddControllers();

var app = builder.Build();

QuestPDF.Settings.License = LicenseType.Community;

// Seeding

if (!builder.Environment.IsEnvironment("Test"))
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    await DbInitializer.SeedRolesAsync(services);

}



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Automation API v1");
});


    app.UseMiddleware<ApiKeyMiddleware>();


app.Use(async (context, next) =>
{
    var org = context.Items["Organization"];
    var userId = context.Items["DefaultAutomationUser"];

    using (Serilog.Context.LogContext.PushProperty("OrgId", org?.GetType().GetProperty("Id")?.GetValue(org)))
    using (Serilog.Context.LogContext.PushProperty("UserId", userId))
    {
        await next();
    }
});

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode}";
});

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

if (!app.Environment.IsEnvironment("Test"))
{
    app.MapStaticAssets();
}
app.MapRazorPages()
   .WithStaticAssets();
app.MapControllers();


app.Run();
