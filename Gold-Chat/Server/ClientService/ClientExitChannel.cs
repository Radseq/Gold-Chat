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

        string ChannelName;

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelsList = null)
        {
            Client = client;
            Received = receive;
        }

        public void Execute()
        {
            prepareResponse();
            ChannelName = Received.strMessage;

            string[] getFromDb = GetChannelFromDB();

            if (getFromDb != null)
            {
                int idChannel = Int32.Parse(getFromDb[0]);
                Int64 adminId = Int64.Parse(getFromDb[1]);

                if (idChannel > 0 && adminId > 0)
                {
                    if (adminId != Client.id)
                        Send.strMessage2 = deleteUserFromChannelDb(idChannel);
                    else Send.strMessage2 = "You cannot exit channel that you created";
                }
                else Send.strMessage2 = "You cannot exit this channel because you not joined";
            }
            else Send.strMessage2 = "Channel not exit or you are not joined to";
        }

        private string[] GetChannelFromDB()
        {
            db.bind(new string[] { "channelName", Received.strMessage, "idUser", Client.id.ToString() });
            db.manySelect("SELECT uc.id_channel, c.id_user_founder FROM channel c, user_channel uc WHERE uc.id_channel = c.id_channel AND c.channel_name = @channelName AND uc.id_user = @idUser");
            return db.tableToRow();
        }

        private void RemoveChannelFromClientChannelList()
        {
            Client.enterChannels.Remove(ChannelName);
            OnClientExitChannel(ChannelName, Client.strName);
        }

        private string deleteUserFromChannelDb(int idChannelDb)
        {
            db.bind(new string[] { "idUser", Client.id.ToString(), "idChannel", idChannelDb.ToString() });
            int deleted = db.executeNonQuery("DELETE FROM user_channel WHERE id_user = @idUser AND id_channel = @idChannel");

            if (deleted > 0)
            {
                RemoveChannelFromClientChannelList();
                return "You are exit from the channel";
            }
            else return "You connot exit: " + ChannelName + " contact to admin.";
        }

        protected virtual void OnClientExitChannel(string channelName, string userName)
        {
            ClientExitChannelEvent?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientNameChannel = userName });
        }
    }
}
