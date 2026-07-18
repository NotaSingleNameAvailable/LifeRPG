using System;
using System.Collections.Generic;

namespace LifeRPG.Core.Models
{
    public class Character
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Stable identifier like 1, 2, 3 in your UI logic if you want it
        public int Cid { get; set; }

        public string Name { get; set; } = string.Empty;

        // For now magician emoji, later you can swap this for image paths
        public string Emoji { get; set; } = "🧙";

        public string Description { get; set; } = string.Empty;

        public int UnlockLevel { get; set; } = 1;

        public string? BonusCategoryName { get; set; }

        public virtual ICollection<UserCharacterProgress> UserProgress { get; set; } = new List<UserCharacterProgress>();
    }
}