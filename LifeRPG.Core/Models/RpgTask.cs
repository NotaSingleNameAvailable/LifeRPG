using System;

namespace LifeRPG.Core.Models
{
    public class RpgTask
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public Guid CategoryId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public int XPValue { get; set; } = 10;

        public bool IsCompleted { get; set; } = false;
        public bool IsRecurring { get; set; } = false;

        public string? RecurrencePattern { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public Guid? AwardedCharacterId { get; set; }

        public virtual User? User { get; set; }
        public virtual Category? Category { get; set; }
    }
}