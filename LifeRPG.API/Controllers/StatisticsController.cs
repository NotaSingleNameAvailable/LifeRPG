using LifeRPG.API.DTOs;
using LifeRPG.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace LifeRPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StatisticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StatisticsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetStatistics(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.CharacterProgress)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found");

            var characters = await _context.Characters
                .AsNoTracking()
                .OrderBy(c => c.Cid)
                .ToListAsync();

            var taskCounts = await _context.Tasks
                .Where(t => t.UserId == userId && t.IsCompleted && t.AwardedCharacterId != null)
                .GroupBy(t => t.AwardedCharacterId)
                .Select(g => new { CharacterId = g.Key, Count = g.Count() })
                .ToListAsync();

            var totalTasks = taskCounts.Sum(t => t.Count);

            var characterStats = characters.Select(c =>
            {
                var progress = user.CharacterProgress.FirstOrDefault(cp => cp.CharacterId == c.Id);
                var taskCount = taskCounts.FirstOrDefault(t => t.CharacterId == c.Id);

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

            return Ok(new StatisticsDto
            {
                StreakCount = user.StreakCount,
                Characters = characterStats
            });
        }
    }
}