using LifeRPG.API.DTOs;
using LifeRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LifeRPG.API.Services
{
    public class CharacterService : ICharacterService
    {
        private readonly AppDbContext _context;

        public CharacterService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CharacterProgressDto>> GetCharactersForUserAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.CharacterProgress)
                    .ThenInclude(cp => cp.Character)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return new List<CharacterProgressDto>();

            var allCharacters = await _context.Characters
                .AsNoTracking()
                .OrderBy(c => c.Cid)
                .ToListAsync();


            var taskCounts = await _context.Tasks
                .Where(t => t.UserId == userId && t.IsCompleted && t.AwardedCharacterId != null)
                .GroupBy(t => t.AwardedCharacterId)
                .Select(g => new { CharacterId = g.Key, Count = g.Count() })
                .ToListAsync();

            return allCharacters.Select(c =>
            {
                var progress = user.CharacterProgress
                    .FirstOrDefault(p => p.CharacterId == c.Id);

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
                    TasksCompleted = taskCount?.Count ?? 0, 
                    BonusCategoryName = c.BonusCategoryName
                };
            }).ToList();
        }

        public async Task<(bool Success, string? ErrorMessage)> SelectCharacterAsync(
            Guid userId, Guid characterId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return (false, "User not found");

            var character = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null)
                return (false, "Character not found");

            if (user.LifeLevel < character.UnlockLevel)
                return (false, "Character is locked");

            user.ActiveCharacterId = characterId;
            await _context.SaveChangesAsync();

            return (true, null);
        }
    }
}