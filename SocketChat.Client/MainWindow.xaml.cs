using System.Windows;

namespace SocketChat.Client
{
    public partial class MainWindow : Window
    {
        private MainViewModel _vm = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            _vm.Connect(
                RemoteIpBox.Text,
                int.Parse(RemotePortBox.Text),
                LocalIpBox.Text,
                int.Parse(LocalPortBox.Text),
                UserBox.Text);
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            _vm.Send(MessageBox.Text);
            MessageBox.Clear();
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            _vm.Disconnect();
        }
    }
}