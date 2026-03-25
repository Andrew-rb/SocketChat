using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketChat.Client
{
    public class TcpChatClient
    {
        private Socket? _socket;
        private bool _running;
        private readonly StringBuilder _receiveBuffer = new();

        public event Action<string>? RawMessageReceived;
        public event Action? Disconnected;
        public event Action<string>? SendError;

        public async Task<bool> Connect(string remoteIp, int remotePort, string localIp, int localPort)
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                var localEndPoint = new IPEndPoint(IPAddress.Parse(localIp), localPort);
                _socket.Bind(localEndPoint);

                await _socket.ConnectAsync(new IPEndPoint(IPAddress.Parse(remoteIp), remotePort));
                _running = true;
                _ = Task.Run(ReceiveLoop);
                return true;
            }
            catch
            {
                return false;
            }
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
                        if (newLineIndex < 0) 
                            break;

                        string message = current.Substring(0, newLineIndex);
                        _receiveBuffer.Remove(0, newLineIndex + 1);

                        if (!string.IsNullOrEmpty(message))
                            RawMessageReceived?.Invoke(message);
                    }
                }
            }
            catch { }
            finally
            {
                _running = false;
                Disconnected?.Invoke();
            }
        }

        public void Send(string text)
        {
            try
            {
                var data = Encoding.UTF8.GetBytes(text);
                _socket.Send(data);
            }
            catch (Exception ex)
            {
                SendError?.Invoke($"Ошибка отправки: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            _running = false;
            if (_socket == null) return;

            try
            {
                _socket.LingerState = new LingerOption(true, 1);
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            catch { }
        }
    }
}