public class CharacterResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Emoji { get; set; } = "";
    public string Description { get; set; } = "";

    public int UnlockLevel { get; set; }
    public bool IsUnlocked { get; set; }
    public bool IsActive { get; set; }
}