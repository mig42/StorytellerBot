using Microsoft.EntityFrameworkCore;
using StorytellerBot.Models.Data;

namespace StorytellerBot.Data;

public class AdventureContext : DbContext
{
    public AdventureContext(DbContextOptions<AdventureContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<CommandProgress> CommandProgresses { get; set; } = null!;
    public DbSet<Adventure> Adventures { get; set; } = null!;
    public DbSet<SavedStatus> SavedStatuses { get; set; } = null!;
    public DbSet<CurrentGame> CurrentGames { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>();
        modelBuilder.Entity<CommandProgress>();
        modelBuilder.Entity<Adventure>();
        modelBuilder.Entity<SavedStatus>().HasIndex(s => new { s.UserId, s.AdventureId }).IsUnique();
        modelBuilder.Entity<CurrentGame>()
            .HasKey(cg => new { cg.UserId, cg.SavedStatusId });
    }
}
