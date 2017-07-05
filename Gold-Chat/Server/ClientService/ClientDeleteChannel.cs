using CommandClient;
using Server.ResponseMessages;
using System;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientDeleteChannel : ServerResponds, IPrepareRespond
    {
        public event EventHandler<ClientEventArgs> ClientDeleteChannelEvent;
        //list of all channels
        List<Channel> ChannelsList;
        List<Client> ListOfClientsOnline;
        List<Client> UsersThatEnterToThisChanel;

        DataBaseManager db = DataBaseManager.Instance;

        string channelName;
        bool isChannelExists = false;

        Channel channelToDelete;

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
            channelName = Received.strMessage;
            string adminPass = Received.strMessage2;

            Send.strMessage2 = null; // We dont want send admin password to clients

            db.bind(new string[] { "channelName", channelName, "idUserFounder", Client.id.ToString() });

            string admPass = db.singleSelect("SELECT admin_password FROM channel WHERE channel_name = @channelName AND id_user_founder = @idUserFounder");
            if (adminPass != "")
            {
                if (adminPass == admPass)
                    deleteChannelFromDb(channelName);// Delete channel from db
                else
                    Send.strMessage = "Wrong admin Password for delete Your Channel:" + channelName + "";
            }
            else
                Send.strMessage = "You cannot delete channel that you not own";
        }

        private void deleteChannelFromDb(string channelName)
        {
            db.bind(new string[] { "channelName", channelName, "idUser", Client.id.ToString() });
            int deleteChannelResult = db.delUpdateInsertDb("DELETE FROM channel c, user_channel uc WHERE c.id_channel = uc.id_channel AND c.channel_name = @channelName AND c.id_user_founder = @idUser");

            if (deleteChannelResult > 0)
            {
                channelToDelete = ChannelGets.getChannelByName(ChannelsList, channelName);
                if (channelToDelete != null)
                {
                    UsersThatEnterToThisChanel = ClientGets.getClientEnterChannels(ListOfClientsOnline, channelName);
                    isChannelExists = true;
                }
                else Send.strMessage = "You channel not exists";
            }
            else Send.strMessage = "You cannot delete your channel by exit with unknown reason (error).";
        }

        private void usersEnteredChannelsDelete_DeletedChannel() // i mean clint have list of channel where entered and users need delete that deleted channel
        {
            Client client = ClientGets.getClientEnterChannel(UsersThatEnterToThisChanel, channelName);
            if (client != null)
                client.enterChannels.Remove(channelName);

        }

        public override void RespondToClient()
        {
            if (isChannelExists)
            {
                SendMessageToChannel sendToChannel = new SendMessageToChannel(Send, ListOfClientsOnline, channelName);
                sendToChannel.ResponseToChannel();

                usersEnteredChannelsDelete_DeletedChannel();
                ChannelsList.Remove(channelToDelete);

                OnClientDeleteChannel(channelName, Client.strName);
            }
            base.RespondToClient();
        }

        protected virtual void OnClientDeleteChannel(string channelName, string userName)
        {
            ClientDeleteChannelEvent?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientNameChannel = userName });
        }
    }
}
