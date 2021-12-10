using System.Net;
using ServerCore;

namespace Server
{
    class Program
    {
        static Listener _listener = new Listener();
        public static GameRoom Room = new GameRoom();

        static void Main(string[] args)
        {
            // DNS : Domain Name System.
            var host = Dns.GetHostName();
            var ipHost = Dns.GetHostEntry(host);
            var ipAddress = ipHost.AddressList[0];
            var ipEndPoint = new IPEndPoint(ipAddress, 7777);

            _listener.Start(ipEndPoint, () => { return SessionManager.Instance.Generate(); });
            while (true) {; }
        }
    }
}
