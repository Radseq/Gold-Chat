using CommandClient;
using Server.ResponseMessages;
using System;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientEnterChannel : ServerResponds, IPrepareRespond
    {
        //list of all channels
        private List<Channel> Channels;
        List<Client> ClientList;

        DataBaseManager db = DataBaseManager.Instance;

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
            Channels = channelList;
            ClientList = clientList;
        }

        public void Execute()
        {
            prepareResponse();
            string channelName = Received.strMessage;

            db.bind(new string[] { "channelName", channelName, "idUser", Client.id.ToString() });
            db.manySelect("SELECT uc.id_channel, c.welcome_message FROM channel c, user_channel uc WHERE uc.id_channel = c.id_channel AND c.channel_name = @channelName AND uc.id_user = @idUser");

            string[] respond = db.tableToRow();
            if (respond != null)
            {
                int id_channel_db = Int32.Parse(respond[0]);
                string motd = respond[1];

                foreach (Channel channel in Channels)
                {
                    if (channel.ChannelName == channelName)
                    {
                        if (!channel.Users.Contains(Received.strName) && (!Client.enterChannels.Contains(channelName)))
                        {
                            channel.Users.Add(Received.strName);
                            Client.enterChannels.Add(channelName);

                            Send.strMessage2 = "enter";
                            Send.strMessage3 = motd;

                            // Because user is in channel now, msg will send to him aswell
                            SendMessageToChannel sendToChannel = new SendMessageToChannel(Send, ClientList, channelName);
                            sendToChannel.ResponseToChannel();
                        }
                        else
                        {
                            userWontJoinToServer("Cannot enter Because you already entered to ", channelName);
                        }
                    }
                }
                //OnClientEnterChannel(channelName, client.strName); //todo
            }
            else
            {
                userWontJoinToServer("Cannot Enter Because you not join to ", channelName);
            }
        }

        private void userWontJoinToServer(string serverMessage, string channelName)
        {
            Send.strMessage2 = "deny";
            Send.strMessage3 = serverMessage + channelName;
            RespondToClient();
        }
    }
}
