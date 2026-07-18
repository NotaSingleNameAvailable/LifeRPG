namespace LifeRPG.API.DTOs
{
    public class AchievementDto
    {
        public string Name { get; set; } = string.Empty;
        public string Emoji { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CompleteTaskResultDto
    {
        public bool ForcedCharacterSwitch { get; set; } = false;
        public string? SwitchedToEmoji { get; set; }
        public string? SwitchedToName { get; set; }
        public List<AchievementDto> NewlyEarned { get; set; } = new();
        public List<AchievementDto> NewlyLost { get; set; } = new();
        public int ActualXpAwarded { get; set; } = 0;
        public int ActualLpAwarded { get; set; } = 0;
    }
}