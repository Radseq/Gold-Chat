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
        List<Client> ClientList;
        DataBaseManager db = DataBaseManager.Instance;


        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
            ClientList = clientList;
        }

        public void Execute()
        {
            prepareResponse();
            clientJoinChannel();
        }

        public void Execute(bool afterCreate)
        {
            prepareResponse();
            clientJoinChannel(afterCreate);
        }

        private void clientJoinChannel(bool afterCreate = false)
        {
            string channelName = Received.strMessage;
            string channelPass = Received.strMessage2;

            db.bind("ChannelName", channelName);
            db.manySelect("SELECT id_channel, welcome_Message, enter_password FROM channel WHERE channel_name = @ChannelName");
            string[] getFromDb = db.tableToRow();
            if (getFromDb != null)
            {
                int idChannel = Int32.Parse(getFromDb[0]);
                string welcomeMsg = getFromDb[1]; // Used for send email notyfication when user login 
                string enterPassword = getFromDb[2];

                if (enterPassword == null)
                    Send.strMessage2 = "Send Password";
                else if (channelPass != enterPassword)
                    Send.strMessage2 = "Wrong Password";
                else
                    insertUserJoinedChannelToDb(idChannel, channelName, afterCreate);
            }
            else Send.strMessage2 = "There is no channel that you want to join.";
        }

        private void insertUserJoinedChannelToDb(int idChannelDb, string channelName, bool afterCreate)
        {
            DateTime theDate = DateTime.Now;
            theDate.ToString("MM-dd-yyyy HH:mm");

            db.bind(new string[] { "idUser", Client.id.ToString(), "idChannel", idChannelDb.ToString(), "joinDate", theDate.ToString() });
            int created = db.delUpdateInsertDb("INSERT INTO user_channel (id_user, id_channel, join_date) " + "VALUES (@idUser, @idChannel, @joinDate)");

            if (created > 0)
            {
                if (!afterCreate)
                {
                    Send.strMessage2 = "You are joinet to channel " + channelName + ".";
                    Send.strMessage3 = "ChannelJoined";
                }
                else
                {
                    Send.cmdCommand = Command.createChannel;
                    Send.strMessage = channelName;
                    Send.strMessage2 = "CreatedChannel";
                    //sendChannelList();
                    SendMessageToAll sendToAll = new SendMessageToAll(Client, Send, ClientList); // Ignored users wont get new channel list
                    sendToAll.ResponseToAll();

                    Send.cmdCommand = Received.cmdCommand;
                    Send.strMessage = "You are create channel (" + channelName + ")";
                    Send.strMessage2 = "CreatedChannel";
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
