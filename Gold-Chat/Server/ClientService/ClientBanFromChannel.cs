using CommandClient;
using Server.Interfaces;
using Server.ResponseMessages;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientBanFromChannel : ServerResponds, IBuildResponse
    {
        DataBaseManager db = DataBaseManager.Instance;

        List<Client> ListOfClientsOnline;
        List<Channel> ChannelsList;

        private bool IsUserBannedSuccesfully;

        string BanReason;
        string ChannelName;
        string UserName;
        string BanTime;
        private Client BannedUser;
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
                        Client bannedUser = ClientGets.getClinetByName(ListOfClientsOnline, UserName);
                        if (bannedUser != null && bannedUser.id != channel.FounderiD)
                        {
                            if (bannedUser.enterChannels.Contains(ChannelName))
                            {
                                if (insertUserBanToDb(bannedUser, BanReason, BanTime, channel))
                                    IsUserBannedSuccesfully = true;
                            }
                        }
                        else
                            Send.strMessage2 = "There is no " + UserName + " in your channel";
                    }
                    else
                        Send.strMessage2 = "There is no " + UserName + " in your channel";
                }
                else
                    Send.strMessage2 = "Only channel founder can ban";
            }
            else
                Send.strMessage2 = "Your Channel not exists";
        }

        private void sendMessageToChannelAboutUserBan()
        {
            SendMessageToChannel sendToChannel = new SendMessageToChannel(Send, ListOfClientsOnline, ChannelName);
            sendToChannel.Send.strMessage2 = " ban from channel reason " + BanReason + " untill " + BanTime;
            sendToChannel.ResponseToChannel();
        }

        private void removeUser(Client client, Channel channel)
        {
            channel.Users.Remove(UserName);
            client.enterChannels.Remove(UserName);
        }

        private bool insertUserBanToDb(Client client, string banReason, string Bantime, Channel channel)
        {
            BannedUser = client;

            db.bind(new string[] { "idUser", client.id.ToString(), "idChannel", channel.ChannelId.ToString(),
                "BanReason", banReason, "StartBanDateTime", Utilities.getDataTimeNow(), "EndBanDateTime", Bantime });

            if (db.delUpdateInsertDb("INSERT INTO channel_user_bans (id_user, id_channel, reason, start_ban, end_ban) " + "VALUES (@idUser, @idChannel, @BanReason, @StartBanDateTime, @EndBanDateTime)") > 0)
                return true;
            Send.strMessage2 = "Cannot ban user with unknown reason.";
            return false;
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

