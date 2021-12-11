using ServerCore;
using System;
using System.Collections.Generic;
using Server.Session;

namespace Server
{
    internal class GameRoom : IJobQueue
    {
        private readonly List<ClientSession> _sessions = new();
        private readonly JobQueue _jobQueue = new();
        private readonly List<ArraySegment<byte>> _pendingList = new();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Flush()
        {
            foreach (var sessionInRoom in _sessions)
            {
                sessionInRoom.Send(_pendingList);
            }

            // Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }

        public void Enter(ClientSession session)
        {
            // 플레이어 추가
            _sessions.Add(session);
            session.Room = this;

            // 신입생한테 모든 플레이어 목록 전송
            var players = new S_PlayerList();
            foreach (var s in _sessions)
            {
                players.players.Add(new S_PlayerList.Player()
                {
                    isSelf =  (s == session),
                    playerId = s.SessionId,
                    posX = s.PosX,
                    posY = s.PosY,
                    posZ = s.PosZ
                });
            }
            session.Send(players.Write());

            // 신입생 입장을 모두에게 알림
            var broadcastEnterGame = new S_BroadcastEnterGame
            {
                playerId = session.SessionId,
                posX = 0,
                posY = 0,
                posZ = 0
            };

            Broadcast(broadcastEnterGame.Write());
        }

        public void Leave(ClientSession session)
        {
            // 플레이어 제거
            _sessions.Remove(session);

            var broadcastLeaveGame = new S_BroadcastLeaveGame();
            broadcastLeaveGame.playerId = session.SessionId;
            Broadcast(broadcastLeaveGame.Write());
        }

        public void Broadcast(ArraySegment<byte> segment)
        {
            _pendingList.Add(segment);
        }

        public void Move(ClientSession session, C_Move movePacket)
        {
            session.PosX = movePacket.posX;
            session.PosY = movePacket.posY;
            session.PosZ = movePacket.posZ;

            var broadcastMove = new S_BroadcastMove();
            broadcastMove.playerId = session.SessionId;
            broadcastMove.posX = session.PosX;
            broadcastMove.posY = session.PosY;
            broadcastMove.posZ = session.PosZ;
            Broadcast(broadcastMove.Write());
        }
    }
}
