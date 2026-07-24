using LifeRPG.API.DTOs;

namespace LifeRPG.API.Services
{
    public interface ICharacterService
    {
        Task<List<CharacterProgressDto>> GetCharactersForUserAsync(Guid userId);
        Task<(bool Success, string? ErrorMessage)> SelectCharacterAsync(Guid userId, Guid characterId);
    }
}