using System;

namespace Server.Interfaces.DeleteChannel
{
    interface IGetAdminPass
    {
        string GetPassword(string ChannelName, Int64 ClientID);
    }
}
