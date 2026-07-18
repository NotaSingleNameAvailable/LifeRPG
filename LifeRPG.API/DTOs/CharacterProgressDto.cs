namespace LifeRPG.API.DTOs;

public class CharacterProgressDto
{
    public required string Id { get; set; }
    public int Cid { get; set; }
    public required string CharacterId { get; set; }

    public required string CharacterName { get; set; }
    public required string CharacterEmoji { get; set; }
    public string Description { get; set; } = string.Empty;

    public int UnlockLevel { get; set; }
    public bool IsUnlocked { get; set; }
    public bool IsActive { get; set; }

    public int TotalXP { get; set; }
    public int CurrentXP { get; set; }
    public int Level { get; set; }
    public int TasksCompleted { get; set; }
    public string? BonusCategoryName { get; set; }
}