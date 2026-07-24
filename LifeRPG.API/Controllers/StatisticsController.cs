using LifeRPG.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LifeRPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetStatistics(Guid userId)
        {
            var result = await _statisticsService.GetStatisticsForUserAsync(userId);

            if (result == null)
                return NotFound("User not found");

            return Ok(result);
        }
    }
}