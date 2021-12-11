using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;
using Server;
using Server.Session;

class PacketHandler
{
    public static void C_LeaveGameHandler(PacketSession session, IPacket packet)
    {
        if (packet is not C_LeaveGame leavePacket || session is not ClientSession clientSession)
            return;

        GameRoom room = clientSession.Room;
        room.Push(() => room.Leave(clientSession));
    }

    public static void C_MoveHandler(PacketSession session, IPacket packet)
    {
        if (packet is not C_Move movePacket || session is not ClientSession clientSession)
            return;

        Console.WriteLine($"{movePacket.posX}, {movePacket.posY}, {movePacket.posZ}");

        GameRoom room = clientSession.Room;
        room.Push(() => room.Move(clientSession, movePacket));
    }
}