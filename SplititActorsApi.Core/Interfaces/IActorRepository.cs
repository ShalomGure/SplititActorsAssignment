using SplititActorsApi.Core.Models;

namespace SplititActorsApi.Core.Interfaces;

/// <summary>
/// Repository interface for actor data access
/// </summary>
public interface IActorRepository
{
    Task<Actor?> GetByIdAsync(int id);
    Task<(List<Actor> actors, int totalCount)> GetAllAsync(string? nameFilter, int? minRank, int? maxRank, int pageNumber, int pageSize);
    Task<Actor> AddAsync(Actor actor);
    Task<Actor> UpdateAsync(Actor actor);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsByRankAsync(int rank, int? excludeId = null);
    Task<List<Actor>> GetAllAsync();
}
