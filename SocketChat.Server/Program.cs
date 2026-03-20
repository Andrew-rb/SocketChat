
namespace SocketChat.Server
{
    class Program
    {
        static void Main()
        {
            Console.Write("Введите IP сервера: ");
            string ip = Console.ReadLine();

            Console.Write("Введите TCP порт сервера: ");
            int tcpPort = int.Parse(Console.ReadLine());

            var server = new TcpChatServer();
            if (!server.Start(ip, tcpPort))
            {
                Console.WriteLine("Ошибка старта сервера.");
                return;
            }

            Console.WriteLine("Нажмите ENTER для остановки сервера...");
            Console.ReadLine();
        }
    }
}