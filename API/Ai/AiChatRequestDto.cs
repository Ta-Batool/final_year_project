namespace API.Ai
{
    public class AiChatRequestDto
    {
        // "doctor" or "patient"
        public string Role { get; set; } = string.Empty;

        // User's question
        public string Message { get; set; } = string.Empty;
    }

    public class AiChatResponseDto
    {
        public string Reply { get; set; } = string.Empty;
    }
}
