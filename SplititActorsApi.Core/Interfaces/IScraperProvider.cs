using SplititActorsApi.Core.Models;

namespace SplititActorsApi.Core.Interfaces;

/// <summary>
/// Interface for scraper providers (IMDb, Rotten Tomatoes, etc.)
/// </summary>
public interface IScraperProvider
{
    string ProviderName { get; }
    Task<List<Actor>> ScrapeActorsAsync();
}
