using HtmlAgilityPack;
using SplititActorsApi.Core.Interfaces;
using SplititActorsApi.Core.Models;

namespace SplititActorsApi.Data.Scrapers;

/// <summary>
/// Scraper for IMDb top actors list
/// </summary>
public class ImdbScraperProvider : IScraperProvider
{
    private const string ImdbUrl = "https://www.imdb.com/list/ls054840033/";
    private readonly HttpClient _httpClient;

    public string ProviderName => "IMDb";

    public ImdbScraperProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        }
    }

    public async Task<List<Actor>> ScrapeActorsAsync()
    {
        var actors = new List<Actor>();

        try
        {

            var html = await _httpClient.GetStringAsync(ImdbUrl);
  

            // Load HTML document
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            // Find all list items containing actors
            var actorNodes = htmlDoc.DocumentNode
                .SelectNodes("//li[@class='ipc-metadata-list-summary-item']");
            if (actorNodes == null || !actorNodes.Any())
            {
                return actors;
            }

            int rank = 1;
            foreach (var node in actorNodes)
            {
                try
                {
                    var actor = ExtractActorFromNode(node, rank);
                    if (actor != null)
                    {
                        actors.Add(actor);
                        rank++;
                    }
                }
                catch (Exception ex)
                {
                    // Log individual actor parsing errors but continue
                    Console.WriteLine($"Error parsing actor at rank {rank}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scraping IMDb: {ex.Message}");
            throw;
        }

        return actors;
    }

    private Actor? ExtractActorFromNode(HtmlNode node, int rank)
    {
        try
        {
            // ---------- TITLE: "2. Marlon Brando" ----------
            var titleNode = node.SelectSingleNode(".//*[@data-testid='nlib-title']//h3");
            if (titleNode == null) return null;

            var titleText = HtmlEntity.DeEntitize(titleNode.InnerText).Trim(); 

            int parsedRank = rank;
            string name = titleText;

            var dotIndex = titleText.IndexOf('.');
            if (dotIndex > 0 &&
                int.TryParse(titleText[..dotIndex].Trim(), out var r))
            {
                parsedRank = r;
                name = titleText[(dotIndex + 1)..].Trim();   
            }

            // ---------- IMAGE URL ----------
            var imageNode = node.SelectSingleNode(".//img[contains(@class,'ipc-image')]");
            var imageUrl = imageNode?.GetAttributeValue("src", "") ?? "";

            var knownForNode = node.SelectSingleNode(".//*[@data-testid='nlib-known-for-title']");
            var knownFor = new List<string>();

            if (knownForNode != null)
            {
                var knownForTitle = HtmlEntity.DeEntitize(knownForNode.InnerText.Trim());
                if (!string.IsNullOrWhiteSpace(knownForTitle))
                    knownFor.Add(knownForTitle);
            }

            var bioNode = node.SelectSingleNode(
                ".//*[@data-testid='dli-item-description']//div[contains(@class,'ipc-html-content-inner-div')]");

            var bio = HtmlEntity.DeEntitize(bioNode?.InnerText?.Trim() ?? "");

            DateTime? birthDate = null;

            return new Actor
            {
                Id = 0,
                Name = name,
                Rank = parsedRank,
                Bio = bio,
                BirthDate = birthDate,
                ImageUrl = imageUrl,
                KnownFor = knownFor,
                Source = "IMDb"
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting actor data: {ex.Message}");
            return null;
        }
    }
}
