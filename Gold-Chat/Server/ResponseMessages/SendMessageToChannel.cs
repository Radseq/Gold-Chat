using CommandClient;
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
            Client client = ClientGets.getClientEnterChannel(ListOfClientsOnline, ChannelName);
            if (client != null)
            {
                Response(Send.ToByte(), client);
            }
        }
    }
}
