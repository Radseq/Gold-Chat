using CommandClient;
using Server.Interfaces;
using Server.Interfaces.ResponseMessages;
using Server.Modules.ResponseMessagesController;
using Server.Utilies;
using System.Collections.Generic;

namespace Server.Controllers
{
    class LeaveChannelController : ServerResponds, IBuildResponse
    {
        List<Channel> ChannelsList;
        List<Client> ListOfClientsOnline;

        string ChannelName;

        private readonly ISendMessageToChannel SendMessageToChannel;

        public LeaveChannelController(ISendMessageToChannel sendMessageToChannel)
        {
            SendMessageToChannel = sendMessageToChannel;
        }

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
            SendMessageToChannel.ResponseToChannel(Send, ListOfClientsOnline, ChannelName);
        }
    }
}
