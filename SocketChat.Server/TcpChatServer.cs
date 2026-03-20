using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace SocketChat.Server
{
    public class TcpChatServer
    {
        private Socket? _listener;
        private Thread? _acceptThread;
        private readonly ConcurrentDictionary<string, ClientHandler> _clients = new();

        public bool Start(string ip, int port)
        {
            try
            {
                _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var localEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                _listener.Bind(localEndPoint);
                _listener.Listen(100);
                Console.WriteLine($"Сервер запущен на {ip}:{port}");

                _acceptThread = new Thread(AcceptLoop);
                _acceptThread.IsBackground = true;
                _acceptThread.Start();
                return true;
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Ошибка порта: {ex.Message}");
                return false;
            }
        }

        private void AcceptLoop()
        {
            while (true)
            {
                var socket = _listener.Accept();
                Console.WriteLine("Новое подключение");
                var handler = new ClientHandler(socket, this);
                handler.Start();
            }
        }

        public void AddClient(ClientHandler handler) => _clients.TryAdd(handler.Info.IP, handler);
        public void RemoveClient(ClientHandler handler) => _clients.TryRemove(handler.Info.IP, out _);
        public bool IsIpConnected(string ip) => _clients.ContainsKey(ip);
        public IEnumerable<ClientHandler> GetAllClients() => _clients.Values;

        public void Broadcast(string message)
        {
            var snapshot = _clients.Values.ToList();

            foreach (var client in snapshot)
            {
                try
                {
                    client.Send(message);
                }
                catch
                {
                    
                }
            }
        }
    }
}