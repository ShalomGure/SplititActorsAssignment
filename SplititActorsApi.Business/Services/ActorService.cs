using SplititActorsApi.Business.Mappers;
using SplititActorsApi.Core.DTOs;
using SplititActorsApi.Core.Interfaces;

namespace SplititActorsApi.Business.Services;

/// <summary>
/// Business logic service for actor operations
/// </summary>
public class ActorService : IActorService
{
    private readonly IActorRepository _repository;

    public ActorService(IActorRepository repository)
    {
        _repository = repository;
    }

    public async Task<ActorDetailDto?> GetByIdAsync(int id)
    {
        var actor = await _repository.GetByIdAsync(id);
        return actor != null ? ActorMapper.ToDetailDto(actor) : null;
    }

    public async Task<PagedResultDto<ActorSummaryDto>> GetAllAsync(ActorFilterDto filter)
    {
        var (actors, totalCount) = await _repository.GetAllAsync(
            filter.Name,
            filter.MinRank,
            filter.MaxRank,
            filter.PageNumber,
            filter.PageSize
        );

        return new PagedResultDto<ActorSummaryDto>
        {
            Data = actors.Select(ActorMapper.ToSummaryDto).ToList(),
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<ActorDetailDto> CreateAsync(CreateActorDto createDto)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(createDto.Name))
        {
            throw new ArgumentException("Actor name is required.");
        }

        // Check for duplicate rank
        if (await _repository.ExistsByRankAsync(createDto.Rank))
        {
            throw new InvalidOperationException($"An actor with rank {createDto.Rank} already exists.");
        }

        var actor = ActorMapper.ToEntity(createDto);
        var created = await _repository.AddAsync(actor);

        return ActorMapper.ToDetailDto(created);
    }

    public async Task<ActorDetailDto> UpdateAsync(int id, UpdateActorDto updateDto)
    {
        var existingActor = await _repository.GetByIdAsync(id);
        if (existingActor == null)
        {
            throw new KeyNotFoundException($"Actor with ID {id} not found.");
        }

        // Validate name
        if (string.IsNullOrWhiteSpace(updateDto.Name))
        {
            throw new ArgumentException("Actor name is required.");
        }

        // Check for duplicate rank (excluding current actor)
        if (await _repository.ExistsByRankAsync(updateDto.Rank, id))
        {
            throw new InvalidOperationException($"An actor with rank {updateDto.Rank} already exists.");
        }

        ActorMapper.UpdateEntity(existingActor, updateDto);
        var updated = await _repository.UpdateAsync(existingActor);

        return ActorMapper.ToDetailDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
        {
            throw new KeyNotFoundException($"Actor with ID {id} not found.");
        }

        return true;
    }
}
