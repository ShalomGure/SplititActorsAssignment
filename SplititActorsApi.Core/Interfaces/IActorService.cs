using SplititActorsApi.Core.DTOs;

namespace SplititActorsApi.Core.Interfaces;

/// <summary>
/// Service interface for actor business logic
/// </summary>
public interface IActorService
{
    Task<ActorDetailDto?> GetByIdAsync(int id);
    Task<PagedResultDto<ActorSummaryDto>> GetAllAsync(ActorFilterDto filter);
    Task<ActorDetailDto> CreateAsync(CreateActorDto createDto);
    Task<ActorDetailDto> UpdateAsync(int id, UpdateActorDto updateDto);
    Task<bool> DeleteAsync(int id);
}
