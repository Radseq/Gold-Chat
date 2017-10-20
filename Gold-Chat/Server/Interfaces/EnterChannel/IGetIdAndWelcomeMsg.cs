using System;

namespace Server.Interfaces.EnterChannel
{
    interface IGetIdAndWelcomeMsg
    {
        string[] Get(string ChannelName, Int64 ClientId);
    }
}
