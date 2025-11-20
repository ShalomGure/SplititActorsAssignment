using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SplititActorsApi.Core.DTOs;
using SplititActorsApi.Core.Interfaces;

namespace SplititActorsApi.API.Controllers;

/// <summary>
/// API endpoints for managing actors
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ActorsController : ControllerBase
{
    private readonly IActorService _actorService;

    public ActorsController(IActorService actorService)
    {
        _actorService = actorService;
    }

    /// <summary>
    /// Get a paginated list of actors with optional filtering
    /// </summary>
    /// <param name="name">Filter by actor name (partial match)</param>
    /// <param name="minRank">Minimum rank (inclusive)</param>
    /// <param name="maxRank">Maximum rank (inclusive)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of actors (name + id only)</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ActorSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<ActorSummaryDto>>> GetActors(
        [FromQuery] string? name = null,
        [FromQuery] int? minRank = null,
        [FromQuery] int? maxRank = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1)
        {
            return BadRequest("Page number must be greater than 0.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest("Page size must be between 1 and 100.");
        }

        var filter = new ActorFilterDto
        {
            Name = name,
            MinRank = minRank,
            MaxRank = maxRank,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _actorService.GetAllAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Get detailed information for a specific actor
    /// </summary>
    /// <param name="id">Actor ID</param>
    /// <returns>Full actor details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ActorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActorDetailDto>> GetActor(int id)
    {
        var actor = await _actorService.GetByIdAsync(id);
        if (actor == null)
        {
            return NotFound($"Actor with ID {id} not found.");
        }

        return Ok(actor);
    }

    /// <summary>
    /// Create a new actor
    /// </summary>
    /// <param name="createDto">Actor information</param>
    /// <returns>Created actor details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ActorDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ActorDetailDto>> CreateActor([FromBody] CreateActorDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _actorService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetActor), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing actor
    /// </summary>
    /// <param name="id">Actor ID</param>
    /// <param name="updateDto">Updated actor information</param>
    /// <returns>Updated actor details</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ActorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ActorDetailDto>> UpdateActor(int id, [FromBody] UpdateActorDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _actorService.UpdateAsync(id, updateDto);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete an actor
    /// </summary>
    /// <param name="id">Actor ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteActor(int id)
    {
        try
        {
            await _actorService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
