using LifeRPG.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LifeRPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CharacterController : ControllerBase
    {
        private readonly ICharacterService _characterService;

        public CharacterController(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCharacters(Guid userId)
        {
            var characters = await _characterService.GetCharactersForUserAsync(userId);

            if (!characters.Any())
                return NotFound("User not found");

            return Ok(characters);
        }

        [HttpPut("select/{userId}/{characterId}")]
        public async Task<IActionResult> SelectCharacter(Guid userId, Guid characterId)
        {
            var (success, errorMessage) = await _characterService
                .SelectCharacterAsync(userId, characterId);

            if (!success)
                return BadRequest(errorMessage);

            return Ok();
        }
    }
}