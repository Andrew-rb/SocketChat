namespace SocketChat.Client
{
    public class ChatMessage
    {
        public string? Username { get; set; }
        public string? Text { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
        public bool IsSystem { get; set; }

        public override string ToString()
        {
            if (IsSystem)
                return $"[SYS] {Text}";

            return $"[{Time:HH:mm}] {Username}: {Text}";
        }
    }
}