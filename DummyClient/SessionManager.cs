using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    class SessionManager
    {
        public static readonly SessionManager Session = new();
        public static SessionManager Instance => Session;

        private readonly List<ServerSession> _sessions = new();
        private readonly object _lock = new();

        private readonly Random _rand = new();

        public ServerSession Generate()
        {
            lock (_lock)
            {
                var session = new ServerSession();
                _sessions.Add(session);
                return session;
            }
        }

        public void SendForEach()
        {
            lock (_lock)
            {
                foreach (var serverSession in _sessions)
                {
                    var move = new C_Move
                    {
                        posX = _rand.Next(-50, 50),
                        posY = 0.0f,
                        posZ = _rand.Next(-50,50),
                    };
                    serverSession.Send(move.Write());
                }
            }
        }
    }
}
