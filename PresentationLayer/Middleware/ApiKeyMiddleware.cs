using ApplicationLayer.Interfaces.Services;
using DomainLayer.Entities;
using InfrastructureLayer.Context;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace PresentationLayer.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string APIKEY_HEADER = "X-API-KEY";
        private readonly ILogger<ApiKeyMiddleware> _logger;

        public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, LedgerDbContext db)
        {
            var path = context.Request.Path;

            if (!path.StartsWithSegments("/api/v1"))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(APIKEY_HEADER, out var extractedKey))
            {
                _logger.LogWarning("API key missing for request {Path}", context.Request.Path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key missing");
                return;
            }

            var apiKey = extractedKey.ToString().Replace("ApiKey ", "");

            var hashed = HashApiKey(apiKey);

            var org = await db.Set<Organization>()
                              .FirstOrDefaultAsync(o => o.ApiKeyHash == hashed);

            if (org == null)
            {
                _logger.LogWarning("Invalid API key attempt: {ApiKeyHash}", hashed);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid API Key");
                return;
            }

            //store org in HttpContext for controllers
            context.Items["Organization"] = org;
            context.Items["DefaultAutomationUser"] = org.DefaultAutomationUserId;

            await _next(context);
        }

        private string HashApiKey(string apiKey)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(apiKey));
            return Convert.ToBase64String(bytes);
        }
    }
}
