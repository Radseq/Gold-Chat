using CommandClient;
using Server.Interfaces.ResponseMessages;
using Server.Utilies;
using System.Collections.Generic;

namespace Server.Modules.ResponseMessagesController
{
    class SendMessageToChannel : Respond, ISendMessageToChannel
    {
        public void ResponseToChannel(Data Send, List<Client> ListOfClientsOnline, string ChannelName)
        {
            foreach (Client channelClient in ClientGets.getClientsWhoEnterToChannel(ListOfClientsOnline, ChannelName))
                Response(Send.ToByte(), channelClient);
        }
    }
}
