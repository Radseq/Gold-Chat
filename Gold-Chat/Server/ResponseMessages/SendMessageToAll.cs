using System;
using System.Collections.Generic;
using System.Net.Sockets;
using CommandClient;

namespace Server.ResponseMessages
{
    class SendMessageToAll : Respond, IServerSend/*, IServerReceive*/, IClient
    {
        //The collection of all clients logged into the room
        private List<Client> ClientList;

        public Data Send { get; set; }
        public Client Client { get; set; }
        //public Data Received { get; set; }

        public SendMessageToAll(Client client, Data send, List<Client> clientList)
        {
            //Received = new Data(new byte[1024]);
            Send = send;
            ClientList = clientList;
            Client = client;
        }

        //public void prepareResponse()
        //{
        //    Send = Received;
        //}

        public void ResponseToAll()
        {
            foreach (Client cInfo in ClientList)
            {
                if (!cInfo.ignoredUsers.Contains(Client.strName)) // This user(foreach) which get ignored by someone, someone will not see when user: login,logout,msg,msg in channels
                    Response(Send.ToByte(), cInfo);
            }

            //OnClientSendMessage(Send.strMessage); // Server will not see private messages 
        }
    }
}
