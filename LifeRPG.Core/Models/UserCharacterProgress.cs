using System;

namespace LifeRPG.Core.Models
{
    public class UserCharacterProgress
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public Guid CharacterId { get; set; }

        // ===== XP SYSTEM =====
        public int TotalXP { get; set; } = 0;
        public int CurrentXP { get; set; } = 0;
        public int Level { get; set; } = 1;

         public bool IsUnlocked { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastUpdatedAt { get; set; }

        public virtual User User { get; set; } = null!;

        public virtual Character Character { get; set; } = null!;
    }
}