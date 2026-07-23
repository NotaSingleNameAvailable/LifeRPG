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
    public class AchievementController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AchievementController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAchievements(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.UserAchievements)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            var allAchievements = await _context.Achievements
                .OrderBy(a => a.Category)
                .ThenBy(a => a.RequiredValue)
                .ToListAsync();

            var earnedIds = user.UserAchievements
                .Select(ua => ua.AchievementId).ToHashSet();

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
            });

            return Ok(result);
        }
    }
}