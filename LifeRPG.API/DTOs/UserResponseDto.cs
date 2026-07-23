namespace LifeRPG.API.DTOs;

public class UserResponseDto
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
     public string Token { get; set; } = string.Empty;
    public int TotalLifePoints { get; set; }
    public int CurrentLifePoints { get; set; }
    public int LifeLevel { get; set; }
    public int StreakCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ActiveCharacterId { get; set; }
    
    // Character progression (flattened, no circular ref)
    public int CharacterXP { get; set; }
    public int CharacterLevel { get; set; }
}