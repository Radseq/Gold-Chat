using CommandClient;
using Server.ResponseMessages;
using System;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientCreateChannel : ServerResponds, IPrepareRespond
    {
        public event EventHandler<ClientEventArgs> ClientCreateChannelEvent;

        //list of all channels
        private List<Channel> ChannelList;
        List<Client> ClientList;

        DataBaseManager db = DataBaseManager.Instance;

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
            ChannelList = channelList;
            ClientList = clientList;
        }

        public void Execute()
        {
            string roomName = Received.strMessage;

            db.bind(new string[] { "id_user_f", Client.id.ToString(), "channel_n", roomName });
            db.manySelect("SELECT id_user_founder, channel_name FROM channel WHERE id_user_founder = @id_user_f AND channel_name = @channel_n");
            string[] getFromDb = db.tableToRow();

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
                {
                    insertChannelToDb();
                }
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

            DateTime theDate = DateTime.Now;
            theDate.ToString("MM-dd-yyyy HH:mm");

            db.bind(new string[] { "idUser", Client.id.ToString(), "channelName", roomName, "enterPass", enterPassword, "adminPass", adminPassword, "maxUsers", 5.ToString(), "createDate", theDate.ToString(), "welcomeMessage", welcomeMsg });
            int insertChannelIntoDbResult = db.delUpdateInsertDb("INSERT INTO channel (id_user_founder, channel_name, enter_password, admin_password, max_users, create_date, welcome_Message) " +
                "VALUES (@idUser, @channelName, @enterPass, @adminPass, @maxUsers, @createDate, @welcomeMessage)");

            if (insertChannelIntoDbResult > 0)
            {
                Send.strMessage = "You are create channel (" + roomName + ")";
                Send.strMessage2 = "CreatedChannel";

                // Add channel to as channels list
                Channel channel = new Channel(roomName, Client.id);
                ChannelList.Add(channel);
                ClientJoinChannel clientJoinToChannel = new ClientJoinChannel(); // After user create channel we want to make him join
                clientJoinToChannel.Load(Client, Received, ClientList);
                clientJoinToChannel.Execute(true);
                clientJoinToChannel.RespondToClient();
            }
            else
                Send.strMessage = "Channel NOT created with unknown reason.";

            OnClientCreateChannel(roomName, Client.strName);
        }

        //public override void Response()
        //{
        //    if (Send.strMessage2 != "CreatedChannel")
        //        base.Response();
        //}

        protected virtual void OnClientCreateChannel(string channelName, string userName)
        {
            ClientCreateChannelEvent?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientNameChannel = userName });
        }
    }
}
