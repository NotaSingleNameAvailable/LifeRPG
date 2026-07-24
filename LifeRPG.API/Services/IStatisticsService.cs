using LifeRPG.API.DTOs;

namespace LifeRPG.API.Services
{
    public interface IStatisticsService
    {
        Task<StatisticsDto?> GetStatisticsForUserAsync(Guid userId);
    }
}