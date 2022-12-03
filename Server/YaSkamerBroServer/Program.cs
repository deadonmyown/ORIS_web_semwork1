namespace GameServer
{
    class Program
    {
        private static bool _appIsRunning = true;
        
        private static void Main()
        {
            using (var server = new HttpServer())
            {
                server.Start();
                Console.WriteLine("Type command (start / stop / restart / exit / status / switch admin):");
                while (_appIsRunning)
                {
                    ConsoleHandler(Console.ReadLine()?.ToLower(), server);
                }
            }
        }

        private static void ConsoleHandler(string command, HttpServer server)
        {
            switch (command)
            {
                case "start":
                    server.Start();
                    break;
                case "stop":
                    server.Stop();
                    break;
                case "restart":
                    server.Restart();
                    break;
                case "exit":
                    _appIsRunning = false;
                    break;
                case "status":
                    Console.WriteLine($"Server status: {server.ServerStatus.ToString()}");
                    break;
                case "switch admin":
                    server.AdminRules = !server.AdminRules;
                    Console.WriteLine($"Admin rules: {server.AdminRules}");
                    break;
                default:
                    Console.WriteLine("Wrong type");
                    break;
            }
        }
    }
}