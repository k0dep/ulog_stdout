using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web.Script.Serialization;

namespace UnityLogServerListener
{
    class Program
    {
        private const int DEFAULT_PORT = 56223;
        private static JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();

        static void Main(string[] args)
        {
            var port = DEFAULT_PORT;
            var format = "[%i][%t][%l]: %m";

            if(args.Length > 1)
                if (!int.TryParse(args[0], out port))
                {
                    Console.WriteLine("Usage:\nulogproj.exe [port]\nulogproj.exe port format");
                    port = DEFAULT_PORT;
                }

            if (args.Length == 2)
                format = args[1];

            var client = new UdpClient(port);
            IPEndPoint senderEndPoint = null;

            Console.WriteLine($"============ start listen at port: {port} ============");

            while (true)
            {
                string json = null;
                try
                {
                    json = Encoding.UTF8.GetString(client.Receive(ref senderEndPoint));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }

                Message msg = null;

                try
                {
                    msg = jsonSerializer.Deserialize<Message>(json);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }

                WriteLineColored(formatOutput(format, msg), msg.type);
            }
        }

        static string formatOutput(string format, Message msg)
        {
            return format
                .Replace("%i", msg.id.ToString())
                .Replace("%t", msg.time.ToString())
                .Replace("%l", msg.type.ToString())
                .Replace("%m", msg.message)
                .Replace("%s", msg.stacktrace);
        }

        static void WriteLineColored(string msg, LogType type)
        {
            switch (type)
            {
                case LogType.Assert:
                case LogType.Exception:
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case LogType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }

            Console.WriteLine(msg);

            Console.ResetColor();
        }

        class Message
        {
            public uint id;
            public ulong time;
            public LogType type;
            public string message;
            public string stacktrace;
        }

        enum LogType
        {
            Error,
            Assert,
            Warning,
            Log,
            Exception
        }
    }
}
