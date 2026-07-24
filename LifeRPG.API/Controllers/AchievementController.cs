using LifeRPG.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LifeRPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AchievementController : ControllerBase
    {
        private readonly IAchievementService _achievementService;

        public AchievementController(IAchievementService achievementService)
        {
            _achievementService = achievementService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAchievements(Guid userId)
        {
            var result = await _achievementService
                .GetAchievementsForUserAsync(userId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}