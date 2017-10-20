using System;

namespace Server.Interfaces.ExitChannel
{
    interface IGetChannelData
    {
        string[] Get(string ChannelName, Int64 ClientId);
    }
}
