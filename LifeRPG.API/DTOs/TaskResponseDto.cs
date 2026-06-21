namespace LifeRPG.API.DTOs
{
    public class TaskResponseDto
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid CategoryId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int XPValue { get; set; }

        public bool IsCompleted { get; set; }

        public bool IsRecurring { get; set; }

        public string? RecurrencePattern { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? DueDate { get; set; }

        public string? Username { get; set; }

        public string? CategoryName { get; set; }
    }
}