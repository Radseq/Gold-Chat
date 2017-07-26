using CommandClient;
using Server.Interfaces;
using System;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientCreateChannel : ServerResponds, IBuildResponse
    {
        public event EventHandler<ClientEventArgs> ClientCreateChannelEvent;

        //list of all channels
        private List<Channel> ChannelsList;
        List<Client> ListOfClientsOnline;

        DataBaseManager db = DataBaseManager.Instance;

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
            ChannelsList = channelList;
            ListOfClientsOnline = clientList;
        }

        public void Execute()
        {
            string roomName = Received.strMessage;

            db.bind(new string[] { "id_user_f", Client.id.ToString(), "channel_n", roomName });
            db.manySelect("SELECT id_user_founder, channel_name FROM channel WHERE id_user_founder = @id_user_f AND channel_name = @channel_n");
            string[] getFromDb = db.tableToRow();

            Send.strName = Received.strName;
            Send.strMessage2 = "NotCreated";

            if (getFromDb != null)
            {
                int idOfFounderDB = Int32.Parse(getFromDb[0]);
                string channelNameDB = getFromDb[1];

                if (channelNameDB == roomName)
                    Send.strMessage = "Channel Name is in Use, try other.";
                else if (idOfFounderDB != 0)
                    Send.strMessage = "You are create channel before, you can have one channel at time";
                else // User not have channel and name is free
                    insertChannelToDb();
            }
            else insertChannelToDb(); // There is no exists channelName and idfounder, so we can create channel
        }

        private void insertChannelToDb()
        {
            string userName = Received.strName;
            string roomName = Received.strMessage;
            string enterPassword = Received.strMessage2;
            string adminPassword = Received.strMessage3;
            string welcomeMsg = Received.strMessage4;

            prepareResponse();

            db.bind(new string[] { "idUser", Client.id.ToString(), "channelName", roomName, "enterPass", enterPassword, "adminPass",
                adminPassword, "maxUsers", 5.ToString(), "createDate", Utilities.getDataTimeNow(), "welcomeMessage", welcomeMsg });
            int insertChannelIntoDbResult = db.executeNonQuery("INSERT INTO channel (id_user_founder, channel_name, enter_password, admin_password, max_users, create_date, welcome_Message) " +
                "VALUES (@idUser, @channelName, @enterPass, @adminPass, @maxUsers, @createDate, @welcomeMessage)");

            if (insertChannelIntoDbResult > 0)
            {
                Send.strMessage = "You are create channel (" + roomName + ")";
                Send.strMessage2 = "CreatedChannel";
                Send.strMessage3 = null;
                Send.strMessage4 = null;
                // Add channel to as channels list
                ChannelsList.Add(new Channel(db.getLastInsertedID(), roomName, Client.id));

                ClientJoinChannel clientJoinToChannel = new ClientJoinChannel(); // After user create channel we want to make him join
                clientJoinToChannel.Send = Send;
                clientJoinToChannel.Load(Client, Received, ListOfClientsOnline);
                clientJoinToChannel.Execute(db.getLastInsertedID(), roomName);
            }
            else
                Send.strMessage = "Channel NOT created with unknown reason.";

            OnClientCreateChannel(roomName, Client.strName);
        }

        protected virtual void OnClientCreateChannel(string channelName, string userName)
        {
            ClientCreateChannelEvent?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientNameChannel = userName });
        }
    }
}
