using System;

namespace Server.Interfaces.DeleteChannel
{
    interface IDeleteChannel
    {
        bool Delete(string ChannelName, Int64 ClientId);
    }
}
