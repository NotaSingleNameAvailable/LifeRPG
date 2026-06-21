namespace LifeRPG.API.DTOs;

public class UserMeDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;

    public int TotalLifePoints { get; set; }
    public int CurrentLifePoints { get; set; }
    public int LifeLevel { get; set; }

    public string? ActiveCharacterId { get; set; } = string.Empty;

    public int CharacterXP { get; set; }
    public int CharacterLevel { get; set; }

    public List<CharacterProgressDto> Characters { get; set; } = new();
}