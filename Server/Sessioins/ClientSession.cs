using ServerCore;
using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Collections.Generic;

namespace Server
{
	class ClientSession : PacketSession
    {
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            GameRoom room = Program.Room;
            room.Push(
                () => room.Enter(this));
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
            SessionManager.Instance.Remove(this);

            if (Room != null)
            {
                GameRoom room = Room;
                room.Push(
                    () => room.Leave(this));
                Room = null;
            }
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes : {numOfBytes}");
        }
    }

}
