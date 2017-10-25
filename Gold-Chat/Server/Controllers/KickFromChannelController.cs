using CommandClient;
using Server.Interfaces;
using Server.Interfaces.ResponseMessages;
using Server.Modules.ResponseMessagesController;
using Server.Utilies;
using System.Collections.Generic;

namespace Server.Controllers
{
    class KickFromChannelController : ServerResponds, IBuildResponse
    {

        List<Client> ListOfClientsOnline;
        List<Channel> ChannelsList;

        private bool IsUserKickedSuccesfully;
        private readonly ISendMessageToChannel SendMessageToChannel;

        string KickReason;
        string ChannelName;
        string UserName;

        Client kickedUser;
        Channel channel;

        public KickFromChannelController(ISendMessageToChannel sendMessageToChannel)
        {
            SendMessageToChannel = sendMessageToChannel;
        }

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
            ListOfClientsOnline = clientList;
            ChannelsList = channelList;
        }

        public void Execute()
        {
            prepareResponse();

            UserName = Received.strMessage;
            KickReason = Received.strMessage2;
            ChannelName = Received.strMessage3;

            channel = ChannelGets.getChannelByName(ChannelsList, ChannelName);

            if (channel != null)
            {
                if (channel.FounderiD == Client.id)
                {
                    if (channel.Users.Contains(UserName))
                    {
                        kickedUser = ClientGets.getClinetByName(ListOfClientsOnline, UserName);
                        if (kickedUser != null)
                        {
                            if (kickedUser.enterChannels.Contains(ChannelName))
                                IsUserKickedSuccesfully = true;

                        }
                        else Send.strMessage = $"There is no {UserName} online";
                    }
                    else
                        Send.strMessage2 = $"There is no {UserName} in your channel";
                }
                else
                    Send.strMessage2 = "Only channel founder can kick";
            }
            else
                Send.strMessage2 = "Your Channel not exists";
        }

        private void sendMessageToChannelAboutUserKick()
        {
            Data send = Send;
            send.strMessage2 = " kicked from channel by Admin";
            SendMessageToChannel.ResponseToChannel(send, ListOfClientsOnline, ChannelName);
        }

        private void removeUser(Client client, Channel channel)
        {
            channel.Users.Remove(UserName);
            client.enterChannels.Remove(UserName);
        }

        public override void Response()
        {
            if (IsUserKickedSuccesfully)
            {
                sendMessageToChannelAboutUserKick();
                removeUser(kickedUser, channel);
            }
            //base.RespondToClient();
        }
    }
}
