using SplititActorsApi.Core.DTOs;
using SplititActorsApi.Core.Models;

namespace SplititActorsApi.Business.Mappers;

/// <summary>
/// Mapper for converting between Actor entity and DTOs
/// </summary>
public static class ActorMapper
{
    public static ActorDetailDto ToDetailDto(Actor actor)
    {
        return new ActorDetailDto
        {
            Id = actor.Id,
            Name = actor.Name,
            Rank = actor.Rank,
            Bio = actor.Bio,
            BirthDate = actor.BirthDate,
            ImageUrl = actor.ImageUrl,
            KnownFor = actor.KnownFor,
            Source = actor.Source
        };
    }

    public static ActorSummaryDto ToSummaryDto(Actor actor)
    {
        return new ActorSummaryDto
        {
            Id = actor.Id,
            Name = actor.Name
        };
    }

    public static Actor ToEntity(CreateActorDto dto)
    {
        return new Actor
        {
            Name = dto.Name,
            Rank = dto.Rank,
            Bio = dto.Bio,
            BirthDate = dto.BirthDate,
            ImageUrl = dto.ImageUrl,
            KnownFor = dto.KnownFor,
            Source = dto.Source
        };
    }

    public static void UpdateEntity(Actor actor, UpdateActorDto dto)
    {
        actor.Name = dto.Name;
        actor.Rank = dto.Rank;
        actor.Bio = dto.Bio;
        actor.BirthDate = dto.BirthDate;
        actor.ImageUrl = dto.ImageUrl;
        actor.KnownFor = dto.KnownFor;
        actor.Source = dto.Source;
    }
}
