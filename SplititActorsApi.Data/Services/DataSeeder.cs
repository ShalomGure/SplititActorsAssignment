using SplititActorsApi.Core.Interfaces;
using SplititActorsApi.Core.Models;
using SplititActorsApi.Data.DbContexts;

namespace SplititActorsApi.Data.Services;

/// <summary>
/// Service to seed the database with initial data from scraper providers
/// </summary>
public class DataSeeder
{
    private readonly ActorsDbContext _context;
    private readonly IScraperProvider _scraperProvider;

    public DataSeeder(ActorsDbContext context, IScraperProvider scraperProvider)
    {
        _context = context;
        _scraperProvider = scraperProvider;
    }

    public async Task SeedAsync()
    {
        // Check if database is already seeded
        if (_context.Actors.Any())
        {
            Console.WriteLine("Database already contains actors. Skipping seeding.");
            return;
        }

        Console.WriteLine($"Seeding database from {_scraperProvider.ProviderName}...");

        try
        {
            // Scrape actors from provider
            var actors = await _scraperProvider.ScrapeActorsAsync();

            if (actors.Any())
            {
                // Add actors to database
                await _context.Actors.AddRangeAsync(actors);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Successfully seeded {actors.Count} actors from {_scraperProvider.ProviderName}.");
            }
            else
            {
                Console.WriteLine($"Warning: No actors found from {_scraperProvider.ProviderName}.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding database: {ex.Message}");
            throw;
        }
    }
}
