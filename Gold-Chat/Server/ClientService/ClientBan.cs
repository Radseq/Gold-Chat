using CommandClient;
using Server.ResponseMessages;
using System.Collections.Generic;
using System;

namespace Server.ClientService
{
    class ClientBan : ServerResponds, IClient, IPrepareRespond
    {
        DataBaseManager db = DataBaseManager.Instance;
        List<Client> ClientList;

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelsList = null)
        {
            ClientList = clientList;
            Received = receive;
            Client = client;
        }

        public void Execute()
        {
            prepareResponse();
            string userName = Received.strMessage;
            string time = Received.strMessage2;
            string banReason = Received.strMessage3;

            if (Client.permission > 0)
            {
                foreach (Client client in ClientList)
                {
                    if (client.strName == userName && client.permission == 0)
                        if (insertUserBanToDb(client, banReason, time) == 0) Send.strMessage2 = "Cannot ban " + userName + " unknown reason";
                }
            }
            else Send.strMessage = "You dont have permission to kick " + userName;
        }

        private int insertUserBanToDb(Client client, string banReason, string Bantime)
        {
            db.bind(new string[] { "idUser", client.id.ToString(), "BanReason", banReason, "EndBanDateTime", Bantime });

            if (db.delUpdateInsertDb("INSERT INTO user_bans (id_user, reason, end_ban) " + "VALUES (@idUser, @BanReason, @EndBanDateTime)") > 0)
            {
                SendMessageToAll sendToAll = new SendMessageToAll(Client, Send, ClientList);
                sendToAll.ResponseToAll();
                client.cSocket.Close();
                return 1;
            }
            return 0;
        }
    }
}
