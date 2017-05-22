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
        List<Channel> ListOfChannels;
        List<Client> ListOfClientsOnline;

        DataBaseManager db = DataBaseManager.Instance;

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
            ListOfClientsOnline = clientList;
            ListOfChannels = channelList;
        }

        public void Execute()
        {
            prepareResponse();
            string channelName = Received.strMessage;
            string adminPass = Received.strMessage2;

            db.bind(new string[] { "channelName", channelName, "idUserFounder", Client.id.ToString() });

            string admPass = db.singleSelect("SELECT admin_password FROM channel WHERE channel_name = @channelName AND id_user_founder = @idUserFounder");
            if (adminPass != "")
            {
                if (adminPass == admPass)
                    Send.strMessage = deleteChannelFromDb(channelName);// Delete channel from db
                else
                    Send.strMessage = "Wrong admin Password for delete Your Channel:" + channelName + "";
            }
            else
                Send.strMessage = "You cannot delete channel that you not own";
        }

        private string deleteChannelFromDb(string channelName)
        {
            db.bind(new string[] { "channelName", channelName, "idUser", Client.id.ToString() });
            int deleteChannelResult = db.delUpdateInsertDb("DELETE FROM channel c, user_channel uc WHERE c.id_channel = uc.id_channel AND c.channel_name = @channelName AND c.id_user_founder = @idUser");

            if (deleteChannelResult > 0)
            {
                // If channels list have channel, delete this channel from list
                foreach (Channel channel in ListOfChannels)
                {
                    if (channel.ChannelName == channelName)
                        ListOfChannels.Remove(channel);
                }
                Client.enterChannels.Remove(channelName);
                OnClientDeleteChannel(channelName, Client.strName);
                return "You are deleted your channel: " + channelName;

                //TODO message to others users witch are in channel that has deleted by creator
            }
            else return "You cannot delete your channel by exit with unknown reason (error).";
        }

        public void Response()
        {
            if (Send.strMessage2 == "enter")
            {
                SendMessageToChannel sendToChannel = new SendMessageToChannel(Send, ListOfClientsOnline, Received.strMessage/*channelName*/);
                sendToChannel.ResponseToChannel();
            }

            RespondToClient();
        }

        protected virtual void OnClientDeleteChannel(string channelName, string userName)
        {
            ClientDeleteChannelEvent?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientNameChannel = userName });
        }
    }
}
