using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Listener
    {
        private Socket _listenSocket;
        private Func<PacketSession> _sessionFactory;

        public void Start(IPEndPoint endPoint, Func<PacketSession> sessionFactory, int register = 10, int backlog = 10)
        {
            // Create listen socket.
            _listenSocket = new Socket(endPoint.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;

            // socket bind.
            _listenSocket.Bind(endPoint);

            // socket listen.
            // backlog : 최대 대기수
            _listenSocket.Listen(backlog);

            for (int i = 0; i < register; i++)
            {
                // create socket async event.
                // register accept socket.
                var args = new SocketAsyncEventArgs();
                // 스레드 풀에서 가져와서 계속해서 Listen중임.
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
                RegisterAccept(args);
            }
        }

        private void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false)
            {
                OnAcceptCompleted(null, args);
            }
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                PacketSession session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }

            // complete 되었으므로, 다시 한번 Register해줌.
            RegisterAccept(args);
        }
    }
}
