using CommandClient;
using Server.Interfaces;
using Server.ResponseMessages;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientLeaveChannel : ServerResponds, IBuildResponse
    {
        List<Channel> ChannelsList;
        List<Client> ListOfClientsOnline;

        string ChannelName;

        public void Load(Client client, Data receive, List<Client> clientList, List<Channel> channelList)
        {
            Client = client;
            Received = receive;
            ChannelsList = channelList;
            ListOfClientsOnline = clientList;
        }

        public void Execute()
        {
            prepareResponse();
            ChannelName = Received.strMessage;
            Client.enterChannels.Remove(ChannelName);
            // Remove user form channel users
            Channel channel = ChannelGets.getChannelByName(ChannelsList, ChannelName);
            if (channel != null)
                channel.Users.Remove(Client.strName);

            //OnClientLeaveChannel(channelName, client.strName); //todo
        }

        public override void Response()
        {
            base.Response();
            SendMessageToChannel sendToChannel = new SendMessageToChannel(Send, ListOfClientsOnline, ChannelName);
            sendToChannel.ResponseToChannel();
        }
    }
}
