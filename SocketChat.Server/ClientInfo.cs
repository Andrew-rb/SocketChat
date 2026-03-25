using System.Net.Sockets;

namespace SocketChat.Server
{
    public class ClientInfo
    {
        public string? Username { get; set; }
        public string? IP { get; set; }
        public int? Port { get; set; }          
        public Socket? Socket { get; set; }
    }
}