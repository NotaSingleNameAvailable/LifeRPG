// Why return IEnumerable<object>? The result is an anonymous type in the original.
// We keep it as object to preserve the exact same shape without creating a new DTO.
// In a future cleanup this could become a proper AchievementResponseDto.

namespace LifeRPG.API.Services
{
    public interface IAchievementService
    {
        Task<IEnumerable<object>?> GetAchievementsForUserAsync(Guid userId);
    }
}