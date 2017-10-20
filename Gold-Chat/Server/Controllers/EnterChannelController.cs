using CommandClient;
using Server.Interfaces;
using Server.Interfaces.EnterChannel;
using Server.Interfaces.ResponseMessages;
using Server.Modules.ResponseMessagesController;
using Server.Utilies;
using System;
using System.Collections.Generic;

namespace Server.Controllers
{
    class EnterChannelController : ServerResponds, IBuildResponse
    {
        //list of all channels
        private List<Channel> Channels;
        List<Client> ListOfClientsOnline;
        bool IsUserEnter = false;
        string ChannelName;

        private readonly IGetIdAndWelcomeMsg InfoChannel;
        private readonly ISendMessageToChannel SendMessageToChannel;

        public EnterChannelController(IGetIdAndWelcomeMsg infoChannel, ISendMessageToChannel sendMessageToChannel)
        {
            InfoChannel = infoChannel;
            SendMessageToChannel = sendMessageToChannel;
        }

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
            Channels = channelList;
            ListOfClientsOnline = clientList;
        }

        public void Execute()
        {
            prepareResponse();
            ChannelName = Received.strMessage;

            string[] dbData = InfoChannel.Get(ChannelName, Client.id);
            if (dbData != null)
            {
                int id_channel_db = Int32.Parse(dbData[0]);
                string motd = dbData[1];

                Channel channel = ChannelGets.getChannelByName(Channels, ChannelName);

                if (channel != null)
                {
                    if (!channel.Users.Contains(Received.strName) && (!Client.enterChannels.Contains(ChannelName)))
                    {
                        channel.Users.Add(Received.strName);
                        Client.enterChannels.Add(ChannelName);

                        Send.strMessage2 = "enter";
                        Send.strMessage3 = motd;
                        IsUserEnter = true;
                    }
                    else
                        userWontJoinToServer("Cannot enter Because you already entered to ");
                }

                //OnClientEnterChannel(channelName, client.strName); //todo
            }
            else
            {
                userWontJoinToServer("Cannot Enter Because you not join to ");
            }
        }

        private void userWontJoinToServer(string serverMessage)
        {
            Send.strMessage2 = "deny";
            Send.strMessage3 = serverMessage + ChannelName;
            Response();
        }

        public override void Response()
        {
            if (IsUserEnter)
                SendMessageToChannel.ResponseToChannel(Send, ListOfClientsOnline, ChannelName);
            else
                base.Response();
        }
    }
}
