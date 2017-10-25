using CommandClient;
using Server.Interfaces;
using Server.Interfaces.JoinChannel;
using Server.Interfaces.ResponseMessages;
using Server.Modules.ResponseMessagesController;
using System;
using System.Collections.Generic;

namespace Server.Controllers
{
    class JoinChannelController : ServerResponds, IBuildResponse
    {
        public event EventHandler<ClientEventArgs> ClientJoinChannelEvent;

        List<Client> ListOfClientsOnline;

        bool isUserJoinAfterCreate = false;

        private readonly IGetChannelProperties GetChannelProperties;
        private readonly ISaveJoinedUser SaveJoinedUser;
        private readonly ISendMessageToAll SendMessage;

        string ChannelName;

        public JoinChannelController(IGetChannelProperties getChannel, ISaveJoinedUser saveJoinedUser, ISendMessageToAll sendMessage)
        {
            GetChannelProperties = getChannel;
            SaveJoinedUser = saveJoinedUser;
            SendMessage = sendMessage;
        }

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
            ListOfClientsOnline = clientList;
        }

        public void Execute()
        {
            prepareResponse();
            clientJoinChannel();
        }

        //execute only by ClientCreateChannel
        public void Execute(Int64 idCreatedChannel, string channelName)
        {
            isUserJoinAfterCreate = true;
            ChannelName = channelName;
            int created = SaveJoinedUser.Save(Client, idCreatedChannel);
            if (created == 0)
                Send.strMessage2 = $"cannot join to {channelName} with unknown reason.";
        }

        private void clientJoinChannel()
        {
            ChannelName = Received.strMessage;
            string channelPass = Received.strMessage2;

            string[] getFromDb = GetChannelProperties.Get(ChannelName);

            if (getFromDb != null)
            {
                Int64 idChannel = Int64.Parse(getFromDb[0]);
                string welcomeMsg = getFromDb[1]; // Used for send email notyfication when user login 
                string enterPassword = getFromDb[2];

                if (enterPassword == null)
                    Send.strMessage2 = "Send Password";
                else if (channelPass != enterPassword)
                    Send.strMessage2 = "Wrong Password";
                else if (Client.enterChannels.Contains(ChannelName))
                    Send.strMessage2 = "You are already join to channel.";
                else
                {
                    int created = SaveJoinedUser.Save(Client, idChannel);
                    if (created == 0)
                        Send.strMessage2 = $"cannot join to {ChannelName} with unknown reason.";
                }
            }
            else Send.strMessage2 = "There is no channel that you want to join.";
        }

        public override void Response()
        {
            if (!isUserJoinAfterCreate)
            {
                Send.strMessage2 = $"You are joinet to channel {ChannelName}.";
                Send.strMessage3 = "ChannelJoined";
                base.Response();
            }
            else
            {
                Send.strMessage = ChannelName;
                Send.strMessage2 = "CreatedChannel";

                SendMessage.ResponseToAll(Client, Send, ListOfClientsOnline); //ignored users wont get this msg
            }
            OnClientJoinChannel(ChannelName, Client.strName);
        }

        protected virtual void OnClientJoinChannel(string channelName, string userName)
        {
            ClientJoinChannelEvent?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientNameChannel = userName });
        }
    }
}
