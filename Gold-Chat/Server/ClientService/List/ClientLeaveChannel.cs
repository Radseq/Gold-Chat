using CommandClient;
using Server.ResponseMessages;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientLeaveChannel : ServerResponds, IPrepareRespond
    {
        List<Channel> ListOfChannels;
        List<Client> ListOfClientsOnline;

        public void Load(Client client, Data receive, List<Client> clientList, List<Channel> channelList)
        {
            Client = client;
            Received = receive;
            ListOfChannels = channelList;
            ListOfClientsOnline = clientList;
        }

        public void Execute()
        {
            prepareResponse();
            string channelName = Received.strMessage;
            Client.enterChannels.Remove(channelName);
            // Remove user form channel users
            foreach (Channel channel in ListOfChannels)
                if (channel.ChannelName == channelName)
                    channel.Users.Remove(Client.strName);

            //OnClientLeaveChannel(channelName, client.strName); //todo
        }

        public void Response()
        {
            RespondToClient();
            SendMessageToChannel sendToChannel = new SendMessageToChannel(Send, ListOfClientsOnline, Received.strMessage/*ChannelName*/);
            sendToChannel.ResponseToChannel();
        }
    }
}
