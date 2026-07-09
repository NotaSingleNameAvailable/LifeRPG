namespace LifeRPG.API.DTOs
{
    public class CompleteTaskResultDto
    {
        public bool ForcedCharacterSwitch { get; set; } = false;
        public string? SwitchedToEmoji { get; set; }
        public string? SwitchedToName { get; set; }
    }
}