using ServerCore;
using System;
using System.Collections.Generic;

namespace Server
{
    class GameRoom : IJobQueue
    {
        private List<ClientSession> _sessions = new List<ClientSession>();
        private JobQueue _jobQueue = new JobQueue();

        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Room = this;
        }

        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);
        }

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerid = session.SessionId;
            packet.chat = $"{chat} I am {packet.playerid}";

            ArraySegment<byte> segment = packet.Write();

            foreach (var sessionInRoom in _sessions)
            {
                sessionInRoom.Send(segment);
            }
        }

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }
    }
}
