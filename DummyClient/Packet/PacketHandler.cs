using System;
using DummyClient;
using ServerCore;

class PacketHandler
{
    public static void S_BroadcastEnterGameHandler(PacketSession session, IPacket packet)
    {
        if (packet is not S_BroadcastEnterGame pktBroadcastEnterGame || session is not ServerSession serverSession)
            return;
    }

    public static void S_BroadcastLeaveGameHandler(PacketSession session, IPacket packet)
    {
        if (packet is not S_BroadcastLeaveGame pktBroadcastLeaveGame || session is not ServerSession serverSession)
            return;
    }

    public static void S_PlayerListHandler(PacketSession session, IPacket packet)
    {
        if (packet is not S_PlayerList pktPlayerList || session is not ServerSession serverSession)
            return;
    }

    public static void S_BroadcastMoveHandler(PacketSession session, IPacket packet)
    {
        if (packet is not S_BroadcastMove pktBroadcastMove || session is not ServerSession serverSession)
            return;
    }
}
