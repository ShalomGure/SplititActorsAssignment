namespace SplititActorsApi.Core.DTOs;

/// <summary>
/// DTO for creating a new actor
/// </summary>
public class CreateActorDto
{
    public string Name { get; set; } = string.Empty;
    public int Rank { get; set; }
    public string Bio { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public List<string> KnownFor { get; set; } = new();
    public string Source { get; set; } = string.Empty;
}
