using LifeRPG.API.DTOs;
using LifeRPG.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LifeRPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register(
            [FromBody] RegisterRequestDto request)
        {
            var (result, error) = await _userService.RegisterAsync(request);

            if (result == null)
                return BadRequest(error);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserResponseDto>> Login(
            [FromBody] LoginRequestDto request)
        {
            var (result, error) = await _userService.LoginAsync(request);

            if (result == null)
                return Unauthorized(error);

            return Ok(result);
        }

        [HttpGet("{userId}/stats")]
        [Authorize]
        public async Task<ActionResult<UserStatsDto>> GetUserStats(string userId)
        {
            var result = await _userService.GetStatsAsync(userId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPut("{userId}/stats")]
        [Authorize]
        public async Task<IActionResult> UpdateUserStats(
            string userId, [FromBody] UserStatsDto stats)
        {
            var success = await _userService.UpdateStatsAsync(userId, stats);

            if (!success)
                return NotFound();

            return Ok();
        }

        [HttpGet("{userId}/characters")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CharacterProgressDto>>> GetUserCharacters(
            string userId)
        {
            var result = await _userService.GetUserCharactersAsync(userId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("{userId}/me")]
        [Authorize]
        public async Task<ActionResult<UserMeDto>> GetUserState(string userId)
        {
            var result = await _userService.GetUserStateAsync(userId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost("{userId}/apply-task-delta")]
        [Authorize]
        public async Task<IActionResult> ApplyTaskDelta(
            string userId, [FromBody] ApplyTaskDeltaDto dto)
        {
            var (result, error) = await _userService.ApplyTaskDeltaAsync(userId, dto);

            if (result == null)
                return BadRequest(error);

            return Ok(result);
        }
    }
}