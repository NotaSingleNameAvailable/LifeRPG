using LifeRPG.API.DTOs;
using LifeRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LifeRPG.API.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly AppDbContext _context;

        public StatisticsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StatisticsDto?> GetStatisticsForUserAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.CharacterProgress)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null; // controller will return NotFound()

            var characters = await _context.Characters
                .AsNoTracking()
                .OrderBy(c => c.Cid)
                .ToListAsync();

            var taskCounts = await _context.Tasks
                .Where(t => t.UserId == userId && t.IsCompleted && t.AwardedCharacterId != null)
                .GroupBy(t => t.AwardedCharacterId)
                .Select(g => new { CharacterId = g.Key, Count = g.Count() })
                .ToListAsync();

            // totalTasks exists in original even though unused in return,
            // keeping it in case it's needed for future percentage calculations
            var totalTasks = taskCounts.Sum(t => t.Count);

            var characterStats = characters.Select(c =>
            {
                var progress = user.CharacterProgress
                    .FirstOrDefault(cp => cp.CharacterId == c.Id);

                var taskCount = taskCounts
                    .FirstOrDefault(t => t.CharacterId == c.Id);

                return new CharacterProgressDto
                {
                    Id = c.Id.ToString(),
                    CharacterId = c.Id.ToString(),
                    Cid = c.Cid,
                    CharacterName = c.Name,
                    CharacterEmoji = c.Emoji,
                    Description = c.Description,
                    UnlockLevel = c.UnlockLevel,
                    IsUnlocked = user.LifeLevel >= c.UnlockLevel,
                    IsActive = user.ActiveCharacterId == c.Id,
                    TotalXP = progress?.TotalXP ?? 0,
                    CurrentXP = progress?.CurrentXP ?? 0,
                    Level = progress?.Level ?? 1,
                    TasksCompleted = taskCount?.Count ?? 0 
                };
            }).ToList();

            return new StatisticsDto
            {
                StreakCount = user.StreakCount,  
                Characters = characterStats  
            };
        }
    }
}