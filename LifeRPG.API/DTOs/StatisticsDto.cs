namespace LifeRPG.API.DTOs
{
    public class StatisticsDto
    {
        public int StreakCount { get; set; }
        public List<CharacterProgressDto> Characters { get; set; } = new();
    }
}