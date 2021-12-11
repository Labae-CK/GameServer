using System;
using System.Net;
using System.Threading;
using Server.Session;
using ServerCore;

namespace Server
{
    class Program
    {
        private static readonly Listener Listener = new();
        public static GameRoom room = new();

        static void FlushRoom()
        {
            room.Push(() => room.Flush());
            JobTimer.Instance.Push(FlushRoom, 250);
        }

        static void Main(string[] args)
        {
            // DNS : Domain Name System.
            var host = Dns.GetHostName();
            var ipHost = Dns.GetHostEntry(host);
            var ipAddress = ipHost.AddressList[0];
            var ipEndPoint = new IPEndPoint(ipAddress, 7777);

            Listener.Start(ipEndPoint, () => SessionManager.Instance.Generate());
            Console.WriteLine("Listening...");

            JobTimer.Instance.Push(FlushRoom);

            while (true)
            {
                JobTimer.Instance.Flush();
            }
        }
    }
}
