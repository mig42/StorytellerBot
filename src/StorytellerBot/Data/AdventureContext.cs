using Microsoft.EntityFrameworkCore;
using StorytellerBot.Models;

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
        modelBuilder.Entity<User>(entity =>
        {
            entity
                .HasOne(u => u.CommandProgress)
                .WithOne(cp => cp.User)
                .HasForeignKey<User>(u => u.CommandProgressId)
                .OnDelete(DeleteBehavior.SetNull);

            entity
                .HasOne(u => u.CurrentGame)
                .WithOne(cg => cg.User)
                .HasForeignKey<User>(u => u.CurrentGameId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<CommandProgress>();
        modelBuilder.Entity<Adventure>();
        modelBuilder.Entity<SavedStatus>();
        modelBuilder.Entity<CurrentGame>()
            .HasOne(cg => cg.SavedStatus)
            .WithOne()
            .HasForeignKey<CurrentGame>(cg => cg.SavedStatusId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
