using CommandClient;
using Server.Interfaces;
using Server.Interfaces.BanFromChannel;
using Server.Interfaces.ResponseMessages;
using Server.Modules.ResponseMessagesController;
using Server.Utilies;
using System.Collections.Generic;

namespace Server.Controllers
{
    class BanFromChannelController : ServerResponds, IBuildResponse
    {
        List<Client> ListOfClientsOnline;
        List<Channel> ChannelsList;

        private bool IsUserBannedSuccesfully;

        private readonly IInsertUserBan InsertBan;
        private readonly ISendMessageToChannel SendToChannel;

        string BanReason;
        string ChannelName;
        string UserName;
        string BanTime;
        private Client BannedUser;
        Channel channel;

        public BanFromChannelController(ISendMessageToChannel sendToChannel, IInsertUserBan insertBan)
        {
            SendToChannel = sendToChannel;
            InsertBan = insertBan;
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
            BanTime = Received.strMessage2;
            BanReason = Received.strMessage3;
            ChannelName = Received.strMessage4;

            channel = ChannelGets.getChannelByName(ChannelsList, ChannelName);

            if (channel != null)
            {
                if (channel.FounderiD == Client.id)
                {
                    if (channel.Users.Contains(UserName))
                    {
                        BannedUser = ClientGets.getClinetByName(ListOfClientsOnline, UserName);
                        if (BannedUser != null && BannedUser.id != channel.FounderiD)
                        {
                            if (BannedUser.enterChannels.Contains(ChannelName))
                            {
                                if (InsertBan.Insert(BannedUser, BanReason, BanTime, channel))
                                    IsUserBannedSuccesfully = true;
                                else
                                    Send.strMessage2 = "Cannot ban user with unknown reason.";
                            }
                        }
                        else
                            Send.strMessage2 = $"There is no {UserName} in your channel";
                    }
                    else
                        Send.strMessage2 = $"There is no {UserName} in your channel";
                }
                else
                    Send.strMessage2 = "Only channel founder can ban";
            }
            else
                Send.strMessage2 = "Your Channel not exists";
        }

        private void sendMessageToChannelAboutUserBan()
        {
            Data send = Send;
            send.strMessage2 = $" ban from channel reason {BanReason} untill {BanTime}";
            SendToChannel.ResponseToChannel(send, ListOfClientsOnline, ChannelName);
        }

        private void removeUser(Client client, Channel channel)
        {
            channel.Users.Remove(UserName);
            client.enterChannels.Remove(UserName);
        }

        public override void Response()
        {
            if (IsUserBannedSuccesfully)
            {
                sendMessageToChannelAboutUserBan();
                removeUser(BannedUser, channel);
            }
            base.Response();
        }
    }
}

