namespace LifeRPG.API.DTOs
{
    public class CreateTaskDto
    {
        public Guid UserId { get; set; }

        public Guid CategoryId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int XPValue { get; set; } = 10;

        public string? RecurrencePattern { get; set; }

        public DateTime? DueDate { get; set; }
    }
}