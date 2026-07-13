using LifeRPG.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace LifeRPG.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<RpgTask> Tasks { get; set; }
        public DbSet<Character> Characters { get; set; }
        public DbSet<UserCharacterProgress> UserCharacterProgress { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Task relationships (existing)
    modelBuilder.Entity<RpgTask>()
        .HasOne(t => t.User)
        .WithMany(u => u.Tasks)
        .HasForeignKey(t => t.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<RpgTask>()
        .HasOne(t => t.Category)
        .WithMany(c => c.Tasks)
        .HasForeignKey(t => t.CategoryId)
        .OnDelete(DeleteBehavior.Cascade);

    // Character -> UserCharacterProgress
    modelBuilder.Entity<UserCharacterProgress>()
        .HasOne(uc => uc.User)
        .WithMany(u => u.CharacterProgress)
        .HasForeignKey(uc => uc.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<UserCharacterProgress>()
        .HasOne(uc => uc.Character)
        .WithMany(c => c.UserProgress)
        .HasForeignKey(uc => uc.CharacterId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<UserAchievement>()
        .HasOne(ua => ua.User)
        .WithMany(u => u.UserAchievements)
        .HasForeignKey(ua => ua.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<UserAchievement>()
        .HasOne(ua => ua.Achievement)
        .WithMany(a => a.UserAchievements)
        .HasForeignKey(ua => ua.AchievementId)
        .OnDelete(DeleteBehavior.Cascade);
}

    }
}