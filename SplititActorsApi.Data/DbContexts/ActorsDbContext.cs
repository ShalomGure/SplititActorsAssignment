using Microsoft.EntityFrameworkCore;
using SplititActorsApi.Core.Models;

namespace SplititActorsApi.Data.DbContexts;

/// <summary>
/// Database context for actors
/// </summary>
public class ActorsDbContext : DbContext
{
    public ActorsDbContext(DbContextOptions<ActorsDbContext> options) : base(options)
    {
    }

    public DbSet<Actor> Actors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Actor>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Rank)
                .IsRequired();

            entity.HasIndex(e => e.Rank)
                .IsUnique();

            entity.Property(e => e.Bio)
                .HasMaxLength(2000);

            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500);

            // Store KnownFor as JSON (EF Core will handle serialization)
            entity.Property(e => e.KnownFor)
                .HasConversion(
                    v => string.Join(";", v),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
                );
        });
    }
}
