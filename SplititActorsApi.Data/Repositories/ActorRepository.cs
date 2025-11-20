using Microsoft.EntityFrameworkCore;
using SplititActorsApi.Core.Interfaces;
using SplititActorsApi.Core.Models;
using SplititActorsApi.Data.DbContexts;

namespace SplititActorsApi.Data.Repositories;

/// <summary>
/// Repository implementation for actor data access
/// </summary>
public class ActorRepository : IActorRepository
{
    private readonly ActorsDbContext _context;

    public ActorRepository(ActorsDbContext context)
    {
        _context = context;
    }

    public async Task<Actor?> GetByIdAsync(int id)
    {
        return await _context.Actors.FindAsync(id);
    }

    public async Task<(List<Actor> actors, int totalCount)> GetAllAsync(
        string? nameFilter,
        int? minRank,
        int? maxRank,
        int pageNumber,
        int pageSize)
    {
        var query = _context.Actors.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(a => a.Name.Contains(nameFilter));
        }

        if (minRank.HasValue)
        {
            query = query.Where(a => a.Rank >= minRank.Value);
        }

        if (maxRank.HasValue)
        {
            query = query.Where(a => a.Rank <= maxRank.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var actors = await query
            .OrderBy(a => a.Rank)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (actors, totalCount);
    }

    public async Task<List<Actor>> GetAllAsync()
    {
        return await _context.Actors
            .OrderBy(a => a.Rank)
            .ToListAsync();
    }

    public async Task<Actor> AddAsync(Actor actor)
    {
        _context.Actors.Add(actor);
        await _context.SaveChangesAsync();
        return actor;
    }

    public async Task<Actor> UpdateAsync(Actor actor)
    {
        _context.Actors.Update(actor);
        await _context.SaveChangesAsync();
        return actor;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var actor = await _context.Actors.FindAsync(id);
        if (actor == null)
        {
            return false;
        }

        _context.Actors.Remove(actor);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsByRankAsync(int rank, int? excludeId = null)
    {
        var query = _context.Actors.Where(a => a.Rank == rank);

        if (excludeId.HasValue)
        {
            query = query.Where(a => a.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
