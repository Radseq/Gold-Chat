using System;

namespace Server.Interfaces.JoinChannel
{
    interface ISaveJoinedUser
    {
        int Save(Client client, Int64 idChannelDb);
    }
}
