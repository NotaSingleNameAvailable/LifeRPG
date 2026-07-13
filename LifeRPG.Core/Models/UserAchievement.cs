namespace LifeRPG.Core.Models
{
    public class UserAchievement
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid AchievementId { get; set; }
        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
        public virtual User? User { get; set; }
        public virtual Achievement? Achievement { get; set; }
    }
}