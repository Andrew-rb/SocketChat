using System.Net.Sockets;
using System.Text;

namespace SocketChat.Server
{
    public class ClientHandler
    {
        private readonly Socket _socket;
        private readonly TcpChatServer _server;
        private Thread _thread;
        private bool _running = true;
        private readonly StringBuilder _receiveBuffer = new();
        private bool _isLeaving = false;

        public ClientInfo Info { get; private set; }

        public ClientHandler(Socket socket, TcpChatServer server)
        {
            _socket = socket;
            _server = server;
        }

        public void Start()
        {
            _thread = new Thread(ReceiveLoop);
            _thread.IsBackground = true;
            _thread.Start();
        }

        private void ReceiveLoop()
        {
            var buffer = new byte[4096];
            try
            {
                while (_running)
                {
                    int bytes = _socket.Receive(buffer);
                    if (bytes <= 0) break;

                    string chunk = Encoding.UTF8.GetString(buffer, 0, bytes);
                    _receiveBuffer.Append(chunk);

                    while (true)
                    {
                        string current = _receiveBuffer.ToString();
                        int newLineIndex = current.IndexOf('\n');
                        if (newLineIndex < 0) break;

                        string message = current.Substring(0, newLineIndex);
                        _receiveBuffer.Remove(0, newLineIndex + 1);

                        if (!string.IsNullOrEmpty(message))
                        {
                            var msg = MessageProtocol.Parse(message);
                            HandleMessage(msg);
                        }
                    }
                }
            }
            catch { }
            finally
            {
                if (!_isLeaving && Info != null)
                {
                    string username = Info.Username;
                    _server.Broadcast(MessageProtocol.CreateSys($"{username} отключился"));
                }

                if (Info != null)
                    _server.RemoveClient(this);

                try
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();
                }
                catch { }
            }
        }

        private void HandleMessage(ParsedMessage msg)
        {
            switch (msg.Type)
            {
                case MessageType.JOIN:
                    
                    string endpoint = _socket.RemoteEndPoint.ToString();
                    string ip = endpoint.Split(':')[0];
                    int port = int.Parse(endpoint.Split(':')[1]);

                    if (_server.IsClientConnected(ip, port))
                    {
                        Send(MessageProtocol.CreateSys("IP и порт уже подключены"));
                        Disconnect();
                        return;
                    }

                    Info = new ClientInfo
                    {
                        Username = msg.Username,
                        IP = ip,
                        Port = port,
                        Socket = _socket
                    };

                    _server.AddClient(this);

                    foreach (var existing in _server.GetAllClients())
                    {
                        if (existing.Info != null && existing != this)
                        {
                            Send(MessageProtocol.CreateSys($"{existing.Info.Username} уже в чате с {existing.Info.IP}:{existing.Info.Port}"));
                        }
                    }

                    _server.Broadcast(MessageProtocol.CreateSys($"{Info.Username} подключился с {Info.IP}:{Info.Port}"));
                    break;

                case MessageType.MSG:
                    _server.Broadcast(MessageProtocol.CreateMsg(msg.Username, msg.Text));
                    break;

                case MessageType.LEAVE:
                    _isLeaving = true;
                    if (Info != null)
                    {
                        _server.Broadcast(MessageProtocol.CreateSys($"{Info.Username} отключился"));
                        _server.RemoveClient(this);
                    }
                    Disconnect();
                    break;
            }
        }

        public void Send(string text)
        {
            try
            {
                if (_socket == null) return;
                if (!_socket.Connected) return;

                var data = Encoding.UTF8.GetBytes(text);
                _socket.Send(data);
            }
            catch { }
        }

        public void Disconnect()
        {
            if (!_running) return;

            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch { }

            try
            {
                _socket.Close();
            }
            catch { }

            _running = false;
        }
    }
}