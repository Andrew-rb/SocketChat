namespace SocketChat.Client
{
    public enum MessageType
    {
        JOIN,
        MSG,
        LEAVE,
        SYS,
        UNKNOWN
    }

    public class ParsedMessage
    {
        public MessageType Type { get; set; }
        public string? Username { get; set; }
        public string? Text { get; set; }
    }

    public static class MessageProtocol
    {
        private const char Delimiter = '\n';

        public static string CreateJoin(string username) => $"JOIN|{username}{Delimiter}";
        public static string CreateMsg(string username, string text) => $"MSG|{username}|{text}{Delimiter}";
        public static string CreateLeave(string username) => $"LEAVE|{username}{Delimiter}";

        public static ParsedMessage Parse(string raw)
        {
            var parts = raw.Split('|');
            try
            {
                var type = Enum.Parse<MessageType>(parts[0], true);
                return type switch
                {
                    MessageType.JOIN => new ParsedMessage { Type = type, Username = parts[1] },
                    MessageType.MSG => new ParsedMessage { Type = type, Username = parts[1], Text = parts[2] },
                    MessageType.LEAVE => new ParsedMessage { Type = type, Username = parts[1] },
                    MessageType.SYS => new ParsedMessage { Type = type, Text = parts[1] },
                    _ => new ParsedMessage { Type = MessageType.UNKNOWN }
                };
            }
            catch
            {
                return new ParsedMessage { Type = MessageType.UNKNOWN };
            }
        }
    }
}