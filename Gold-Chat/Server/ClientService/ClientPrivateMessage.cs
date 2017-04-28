using CommandClient;
using Server.ResponseMessages;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientPrivateMessage : ServerResponds
    {
        List<Client> ClientList;

        public void Load(Client client, Data receive, List<Client> clientList)
        {
            Client = client;
            Received = receive;
            ClientList = clientList;
        }

        public void Response()
        {
            prepareResponse();
            Send.strMessage = Received.strName + ": " + Received.strMessage;
            RespondToClient();

            SendMessageToNick sendToNick = new SendMessageToNick(Client, ClientList, Send, Received);
            sendToNick.prepareRespond();
            sendToNick.ResponseToNick();
        }
    }
}