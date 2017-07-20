using CommandClient;
using Server.ResponseMessages;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientPrivateMessage : ServerResponds
    {
        List<Client> ListOfClientsOnline;

        public void Load(Client client, Data receive, List<Client> clientList)
        {
            Client = client;
            Received = receive;
            ListOfClientsOnline = clientList;
        }

        public void Response()
        {
            prepareResponse();
            Send.strMessage = Received.strName + ": " + Received.strMessage;
            Respond();

            SendMessageToNick sendToNick = new SendMessageToNick(ListOfClientsOnline, Send);
            sendToNick.Send.strName = Received.strMessage2;
            sendToNick.Send.strMessage2 = Client.strName;

            sendToNick.ResponseToNick();
        }
    }
}