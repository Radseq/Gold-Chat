using CommandClient;
using Server.Interfaces;
using System;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientExitChannel : ServerResponds, IBuildResponse
    {
        public event EventHandler<ClientEventArgs> ClientExitChannelEvent;

        DataBaseManager db = DataBaseManager.Instance;
        EmailSender emailSender = EmailSender.Instance;

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelsList = null)
        {
            Client = client;
            Received = receive;
        }

        public void Execute()
        {
            prepareResponse();
            string channelName = Received.strMessage;

            db.bind(new string[] { "channelName", channelName, "idUser", Client.id.ToString() });
            db.manySelect("SELECT uc.id_channel, c.id_user_founder FROM channel c, user_channel uc WHERE uc.id_channel = c.id_channel AND c.channel_name = @channelName AND uc.id_user = @idUser");
            string[] getFromDb = db.tableToRow();
            if (getFromDb != null)
            {
                int idChannelDb = Int32.Parse(getFromDb[0]);
                Int64 adminId = Int64.Parse(getFromDb[1]);

                if (idChannelDb > 0 && adminId > 0)
                {
                    if (adminId != Client.id)
                        Send.strMessage2 = deleteUserFromChannelDb(channelName, idChannelDb);
                    else Send.strMessage2 = "You cannot exit channel that you created";
                }
                else Send.strMessage2 = "You cannot exit this channel because you not joined";
            }
            else Send.strMessage2 = "Channel not exit or you are not joined to";
        }

        private string deleteUserFromChannelDb(string channelName, int idChannelDb)
        {
            db.bind(new string[] { "idUser", Client.id.ToString(), "idChannel", idChannelDb.ToString() });
            int deleted = db.executeNonQuery("DELETE FROM user_channel WHERE id_user = @idUser AND id_channel = @idChannel");

            if (deleted > 0)
            {
                Client.enterChannels.Remove(channelName);
                OnClientExitChannel(channelName, Client.strName);
                return "You are exit from the channel";
            }
            else return "You connot exit: " + channelName + " contact to admin.";
        }

        protected virtual void OnClientExitChannel(string channelName, string userName)
        {
            ClientExitChannelEvent?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientNameChannel = userName });
        }
    }
}
