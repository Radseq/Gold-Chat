using CommandClient;
using Server.Interfaces;
using Server.ResponseMessages;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientPrivateMessage : ServerResponds, IBuildResponse
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
            Send.strMessage = Received.strName + ": " + Received.strMessage;
        }

        public override void Response()
        {
            base.Response();

            SendMessageToNick sendToNick = new SendMessageToNick(ListOfClientsOnline, Send);
            sendToNick.Send.strName = Received.strMessage2;
            sendToNick.Send.strMessage2 = Client.strName;

            sendToNick.ResponseToNick();
        }
    }
}