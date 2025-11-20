namespace SplititActorsApi.Core.DTOs;

/// <summary>
/// Filter and pagination parameters for actor listing
/// </summary>
public class ActorFilterDto
{
    public string? Name { get; set; }
    public int? MinRank { get; set; }
    public int? MaxRank { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
