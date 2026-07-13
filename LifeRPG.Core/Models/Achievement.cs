namespace LifeRPG.Core.Models
{
    public class Achievement
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Key { get; set; } = string.Empty; // unique identifier e.g. "first_blood"
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Emoji { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // "tasks", "streak", "level"
        public int RequiredValue { get; set; } // threshold number
        public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    }
}