using System.Collections.ObjectModel;
using System.Windows;

namespace SocketChat.Client
{
    public class MainViewModel
    {
        public ObservableCollection<ChatMessage> Messages { get; } = new();
        public ObservableCollection<string> Users { get; } = new();
        private TcpChatClient _client = new();
        private string _username = string.Empty;

        public MainViewModel()
        {
            _client.RawMessageReceived += OnRawMessage;
            _client.Disconnected += OnDisconnected;
            _client.SendError += OnSendError;
        }

        public async void Connect(string remoteIp, int remotePort, string localIp, int localPort, string username)
        {
            _username = username;

            if (localIp == remoteIp)
            {
                AddSystemMessage("Ошибка: локальный IP не может совпадать с IP сервера");
                return;
            }
            if (localIp == remoteIp && localPort == remotePort)
            {
                AddSystemMessage("Ошибка: локальный IP и порт не могут совпадать с серверными");
                return;
            }

            var ok = await _client.Connect(remoteIp, remotePort, localIp, localPort);

            if (!ok)
            {
                AddSystemMessage("Ошибка подключения (возможно, локальный порт занят или сервер недоступен)");
                return;
            }
            _client.Send(MessageProtocol.CreateJoin(_username));
        }

        public void Send(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) 
                return;
            _client.Send(MessageProtocol.CreateMsg(_username, text));
        }

        public void Disconnect()
        {
            _client.Send(MessageProtocol.CreateLeave(_username));
            _client.Disconnect();

            Users.Clear();
        }

        private void OnRawMessage(string raw)
        {
            var msg = MessageProtocol.Parse(raw);
            Application.Current.Dispatcher.Invoke(() =>
            {
                switch (msg.Type)
                {
                    case MessageType.MSG:
                        Messages.Add(new ChatMessage
                        {
                            Username = msg.Username,
                            Text = msg.Text
                        });
                        break;

                    case MessageType.SYS:
                        AddSystemMessage(msg.Text);

                        if (msg.Text != null)
                        {
                            string cleanText = msg.Text.TrimEnd('.', ' ');
                            if (cleanText.Contains("подключился"))
                            {
                                var parts = cleanText.Split(' ');
                                if (parts.Length > 0)
                                {
                                    string newUser = parts[0];
                                    if (!Users.Contains(newUser))
                                        Users.Add(newUser);
                                }
                            }
                            else if (cleanText.Contains("отключился"))
                            {
                                var parts = cleanText.Split(' ');
                                if (parts.Length > 0)
                                {
                                    string leftUser = parts[0];
                                    Users.Remove(leftUser);
                                }
                            }
                            else if (cleanText.Contains("уже в чате"))
                            {
                                var parts = cleanText.Split(' ');
                                if (parts.Length > 0)
                                {
                                    string existingUser = parts[0];
                                    if (!Users.Contains(existingUser))
                                        Users.Add(existingUser);
                                }
                            }
                        }
                        break;
                }
            });
        }

        private void OnDisconnected()
        {
            Application.Current.Dispatcher.Invoke(() => AddSystemMessage("Отключён от сервера"));
        }

        private void OnSendError(string error)
        {
            Application.Current.Dispatcher.Invoke(() => AddSystemMessage(error));
        }

        private void AddSystemMessage(string text)
        {
            Messages.Add(new ChatMessage { Text = text, IsSystem = true });
        }
    }
}