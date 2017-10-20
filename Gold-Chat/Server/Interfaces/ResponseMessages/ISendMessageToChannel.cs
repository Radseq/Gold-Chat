using CommandClient;
using System.Collections.Generic;

namespace Server.Interfaces.ResponseMessages
{
    interface ISendMessageToChannel
    {
        void ResponseToChannel(Data send, List<Client> clientList, string channelName);
    }
}
