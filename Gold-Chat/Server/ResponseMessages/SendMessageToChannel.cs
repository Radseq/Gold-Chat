using CommandClient;
using Server.Interfaces;
using System.Collections.Generic;

namespace Server.ResponseMessages
{
    class SendMessageToChannel : Respond, IServerSend
    {
        List<Client> ListOfClientsOnline;
        string ChannelName;

        public Data Send { get; set; }

        public SendMessageToChannel(Data send, List<Client> clientList, string channelName)
        {
            Send = send;
            ListOfClientsOnline = clientList;
            ChannelName = channelName;
        }

        public void ResponseToChannel()
        {
            foreach (Client channelClient in ClientGets.getClientsWhoEnterToChannel(ListOfClientsOnline, ChannelName))
                Response(Send.ToByte(), channelClient);
        }
    }
}
