using CommandClient;
using Server.ResponseMessages;
using System;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientJoinChannel : ServerResponds, IPrepareRespond
    {
        public event EventHandler<ClientEventArgs> ClientJoinChannelEvent;

        //list of all channels
        //private List<Channel> channels;
        List<Client> ListOfClientsOnline;
        DataBaseManager db = DataBaseManager.Instance;

        bool isUserJoinAfterCreate = false;

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

        public void Execute(Int64 idCreatedChannel, string channelName)
        {
            isUserJoinAfterCreate = true;
            insertUserJoinedChannelToDb(idCreatedChannel, channelName);
        }

        private void clientJoinChannel()
        {
            string channelName = Received.strMessage;
            string channelPass = Received.strMessage2;

            db.bind("ChannelName", channelName);
            db.manySelect("SELECT id_channel, welcome_Message, enter_password FROM channel WHERE channel_name = @ChannelName");
            string[] getFromDb = db.tableToRow();
            if (getFromDb != null)
            {
                Int64 idChannel = Int64.Parse(getFromDb[0]);
                string welcomeMsg = getFromDb[1]; // Used for send email notyfication when user login 
                string enterPassword = getFromDb[2];

                if (enterPassword == null)
                    Send.strMessage2 = "Send Password";
                else if (channelPass != enterPassword)
                    Send.strMessage2 = "Wrong Password";
                else if (Client.enterChannels.Contains(channelName))
                    Send.strMessage2 = "You are already join to channel.";
                else
                    insertUserJoinedChannelToDb(idChannel, channelName);
            }
            else Send.strMessage2 = "There is no channel that you want to join.";
        }

        private void insertUserJoinedChannelToDb(Int64 idChannelDb, string channelName)
        {
            db.bind(new string[] { "idUser", Client.id.ToString(), "idChannel", idChannelDb.ToString(), "joinDate", Utilities.getDataTimeNow() });
            int created = db.delUpdateInsertDb("INSERT INTO user_channel (id_user, id_channel, join_date) " + "VALUES (@idUser, @idChannel, @joinDate)");

            if (created > 0)
            {
                if (!isUserJoinAfterCreate)
                {
                    Send.strMessage2 = "You are joinet to channel " + channelName + ".";
                    Send.strMessage3 = "ChannelJoined";
                }
                else
                {
                    Send.strMessage = channelName;
                    Send.strMessage2 = "CreatedChannel";

                    SendMessageToAll sendToAll = new SendMessageToAll(Client, Send, ListOfClientsOnline); // Ignored users wont get new channel list
                    sendToAll.ResponseToAll();
                }
                OnClientJoinChannel(channelName, Client.strName);
            }
            else
                Send.strMessage2 = "cannot join to " + channelName + " with unknown reason.";
        }

        protected virtual void OnClientJoinChannel(string channelName, string userName)
        {
            ClientJoinChannelEvent?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientNameChannel = userName });
        }
    }
}
