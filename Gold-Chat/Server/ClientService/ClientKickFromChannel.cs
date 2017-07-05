using CommandClient;
using Server.ResponseMessages;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientKickFromChannel : ServerResponds, IPrepareRespond
    {
        DataBaseManager db = DataBaseManager.Instance;

        List<Client> ListOfClientsOnline;
        List<Channel> ChannelsList;

        private bool IsUserKickedSuccesfully;

        string KickReason;
        string ChannelName;
        string UserName;

        Client kickedUser;
        Channel channel;

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
                        else Send.strMessage = "There is no " + UserName + " online";
                    }
                    else
                        Send.strMessage2 = "There is no " + UserName + " in your channel";
                }
                else
                    Send.strMessage2 = "Only channel founder can kick";
            }
            else
                Send.strMessage2 = "Your Channel not exists";
        }

        private void sendMessageToChannelAboutUserKick()
        {
            SendMessageToChannel sendToChannel = new SendMessageToChannel(Send, ListOfClientsOnline, ChannelName);
            sendToChannel.Send.strMessage2 = " kicked from channel by Admin";
            sendToChannel.ResponseToChannel();
        }

        private void removeUser(Client client, Channel channel)
        {
            channel.Users.Remove(UserName);
            client.enterChannels.Remove(UserName);
        }

        public override void RespondToClient()
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
