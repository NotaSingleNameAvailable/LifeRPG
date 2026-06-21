using System;
using System.Collections.Generic;

namespace LifeRPG.Core.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        // ===== LP SYSTEM =====
        public int TotalLifePoints { get; set; } = 0;
        public int CurrentLifePoints { get; set; } = 0;
        public int LifeLevel { get; set; } = 1;

        public int StreakCount { get; set; } = 0;

        public DateTime? LastTaskDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public Guid? ActiveCharacterId { get; set; }

        public virtual ICollection<UserCharacterProgress> CharacterProgress { get; set; } = new List<UserCharacterProgress>();

        public virtual ICollection<RpgTask> Tasks { get; set; } = new List<RpgTask>();
    }
}