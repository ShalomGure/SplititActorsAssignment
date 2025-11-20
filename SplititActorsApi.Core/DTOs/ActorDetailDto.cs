namespace SplititActorsApi.Core.DTOs;

/// <summary>
/// Detailed view of actor (used for GET /actors/{id} endpoint)
/// </summary>
public class ActorDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Rank { get; set; }
    public string Bio { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public List<string> KnownFor { get; set; } = new();
}
