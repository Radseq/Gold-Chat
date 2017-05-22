using CommandClient;
using Server.ResponseMessages;
using System.Collections.Generic;
using System;

namespace Server.ClientService
{
    class ClientKick : ServerResponds, IPrepareRespond
    {
        List<Client> ListOfClientsOnline;

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
            ListOfClientsOnline = clientList;
        }

        public void Execute()
        {
            prepareResponse();
            string userName = Received.strMessage;       // Nick of kicked user
            string kickReason = Received.strMessage2;    // Reason of kick

            if (Client.permission > 0)
            {
                foreach (Client client in ListOfClientsOnline)
                {
                    if (client.strName == userName && client.permission == 0)
                    {
                        SendMessageToAll sendToAll = new SendMessageToAll(Client, Send, ListOfClientsOnline); // Ignored users wont get new channel list
                        sendToAll.ResponseToAll();
                        client.cSocket.Close();
                    }
                }
            }
            else Send.strMessage = "You cannot kick " + userName + " because you dont have permissions";
        }
    }
}
