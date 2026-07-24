// Note: Register and Login return UserResponseDto because they need to include
// the JWT token in the response. The service handles token generation too
// since it needs JwtService injected alongside AppDbContext.

using LifeRPG.API.DTOs;

namespace LifeRPG.API.Services
{
    public interface IUserService
    {
        Task<(UserResponseDto? Result, string? ErrorMessage)> RegisterAsync(RegisterRequestDto request);
        Task<(UserResponseDto? Result, string? ErrorMessage)> LoginAsync(LoginRequestDto request);
        Task<UserStatsDto?> GetStatsAsync(string userId);
        Task<bool> UpdateStatsAsync(string userId, UserStatsDto stats);
        Task<IEnumerable<CharacterProgressDto>?> GetUserCharactersAsync(string userId);
        Task<UserMeDto?> GetUserStateAsync(string userId);
        Task<(object? Result, string? ErrorMessage)> ApplyTaskDeltaAsync(string userId, ApplyTaskDeltaDto dto);
    }
}