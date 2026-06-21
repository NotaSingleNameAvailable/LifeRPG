using LifeRPG.API.DTOs;
using LifeRPG.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeRPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharacterController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CharacterController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCharacters(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.CharacterProgress)
                    .ThenInclude(cp => cp.Character)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found");

            var characters = await _context.Characters
                .AsNoTracking()
                .OrderBy(c => c.Cid)
                .ToListAsync();

            var result = characters.Select(c =>
            {
                var progress = user.CharacterProgress.FirstOrDefault(cp => cp.CharacterId == c.Id);

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
                    Level = progress?.Level ?? 1
                };
            }).ToList();

            return Ok(result);
        }

        [HttpPut("select/{userId}/{characterId}")]
        public async Task<IActionResult> SelectCharacter(Guid userId, Guid characterId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound("User not found");

            var character = await _context.Characters.FirstOrDefaultAsync(c => c.Id == characterId);
            if (character == null)
                return NotFound("Character not found");

            if (user.LifeLevel < character.UnlockLevel)
                return BadRequest("Character is locked");

            user.ActiveCharacterId = characterId;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}