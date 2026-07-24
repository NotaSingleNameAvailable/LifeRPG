using LifeRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LifeRPG.API.Services
{
    public class AchievementService : IAchievementService
    {
        private readonly AppDbContext _context;

        public AchievementService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>?> GetAchievementsForUserAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.UserAchievements)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null; // controller will return NotFound()

            var allAchievements = await _context.Achievements
                .OrderBy(a => a.Category)
                .ThenBy(a => a.RequiredValue)
                .ToListAsync();

            // earnedIds HashSet for O(1) lookup
            var earnedIds = user.UserAchievements
                .Select(ua => ua.AchievementId)
                .ToHashSet();

            var result = allAchievements.Select(a => new
            {
                a.Id,
                a.Key,
                a.Name,
                a.Emoji,
                a.Description,
                a.Category,
                a.RequiredValue,
                IsEarned = earnedIds.Contains(a.Id),
                EarnedAt = user.UserAchievements
                    .FirstOrDefault(ua => ua.AchievementId == a.Id)?.EarnedAt
            } as object); // cast to object to match interface return type

            return result;
        }
    }
}