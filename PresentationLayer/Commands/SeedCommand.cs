using InfrastructureLayer.Context;
using PresentationLayer.DbConfiguration;

namespace PresentationLayer.Commands
{
    public static class SeedCommand
    {
        public static async Task ExecuteAsync(IHost app, string[] args)
        {
            if (args.Contains("--seed-dev"))
            {
                using var scope = app.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<LedgerDbContext>();
                await DevelopmentSeeder.SeedDevelopmentDataAsync(context, scope.ServiceProvider);
                Console.WriteLine("Seeding complete!");
                Environment.Exit(0);
            }
        }
    }
}