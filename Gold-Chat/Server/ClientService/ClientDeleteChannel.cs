using CommandClient;
using Server.Interfaces;
using Server.ResponseMessages;
using System;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientDeleteChannel : ServerResponds, IBuildResponse
    {
        public event EventHandler<ClientEventArgs> ClientDeleteChannelEvent;
        //list of all channels
        List<Channel> ChannelsList;
        List<Client> ListOfClientsOnline;
        List<Client> ListOfUsersThatEnterToThisChanel;

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

            Send.strMessage2 = "Deny";

            if (adminPass == GetAdminPassFromDB())
                deleteChannel(channelName);
            else
                Send.strMessage = "Wrong admin Password for delete Your Channel:" + channelName + "";
        }

        private string GetAdminPassFromDB()
        {
            db.bind(new string[] { "channelName", channelName, "idUserFounder", Client.id.ToString() });
            return db.singleSelect("SELECT admin_password FROM channel WHERE channel_name = @channelName AND id_user_founder = @idUserFounder");
        }

        private void deleteChannel(string channelName)
        {
            if (DeleteChannelFromDB() > 0)
            {
                channelToDelete = ChannelGets.getChannelByName(ChannelsList, channelName);
                if (channelToDelete != null)
                {
                    ListOfUsersThatEnterToThisChanel = ClientGets.getClientsWhoEnterToChannel(ListOfClientsOnline, channelName);
                    isChannelExists = true;

                    Send.strMessage2 = Client.strName;
                }
                else Send.strMessage = "You channel not exists";
            }
            else Send.strMessage = "You cannot delete your channel by exit with unknown reason (error).";
        }

        private int DeleteChannelFromDB()
        {
            db.bind(new string[] { "channelName", channelName, "idUser", Client.id.ToString() });
            return db.executeNonQuery("DELETE FROM channel WHERE channel.channel_name = @channelName AND channel.id_user_founder = @idUser");
            // user_channel will be also deleted cuse of - on delete cascade
        }

        private void usersEnteredChannelsDeleteChannel() // i mean clint have list of channel where entered and users need delete that deleted channel
        {
            Client client = ClientGets.getClientEnterChannel(ListOfUsersThatEnterToThisChanel, channelName);
            if (client != null)
                client.enterChannels.Remove(channelName);
        }

        private void ResponseToChannelAboutDeleteThisChannel()
        {
            SendMessageToChannel sendToChannel = new SendMessageToChannel(Send, ListOfClientsOnline, channelName);
            sendToChannel.ResponseToChannel();
        }

        public override void Response()
        {
            if (isChannelExists)
            {
                ResponseToChannelAboutDeleteThisChannel();

                usersEnteredChannelsDeleteChannel();
                ChannelsList.Remove(channelToDelete);

                OnClientDeleteChannel(channelName, Client.strName);
            }
            else
                base.Response();
        }

        protected virtual void OnClientDeleteChannel(string channelName, string userName)
        {
            ClientDeleteChannelEvent?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientNameChannel = userName });
        }
    }
}
