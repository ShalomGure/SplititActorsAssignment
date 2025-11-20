namespace SplititActorsApi.Core.DTOs;

/// <summary>
/// Summary view of actor (used for GET /actors list endpoint)
/// </summary>
public class ActorSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
