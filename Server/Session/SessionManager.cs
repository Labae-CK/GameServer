using System;
using System.Collections.Generic;

namespace Server.Session
{
    internal class SessionManager
    {
        public static SessionManager Instance { get; } = new();

        private int _sessionId;
        private readonly Dictionary<int, ClientSession> _sessions = new();
        private readonly object _lock = new();

        public ClientSession Generate()
        {
            lock (_lock)
            {
                var sessionId = ++_sessionId;

                var session = new ClientSession
                {
                    SessionId = sessionId
                };
                _sessions.Add(sessionId, session);

                // Console.WriteLine($"Conennected : {sessionId}");

                return session;
            }
        }

        public ClientSession Find(int id)
        {
            lock (_lock)
            {
                _sessions.TryGetValue(id, out var session);

                return session;
            }
        }

        public void Remove(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session.SessionId);
            }
        }
    }
}
