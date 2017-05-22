using CommandClient;
using System.Collections.Generic;

namespace Server.ResponseMessages
{
    class SendMessageToNick : Respond, IServerSend, IServerReceive, IClient
    {
        List<Client> ListOfClientsOnline;

        public Data Send { get; set; }
        public Data Received { get; set; }
        public Client Client { get; set; }

        public SendMessageToNick(Client client, List<Client> clientList, Data send, Data received)
        {
            Client = client;
            ListOfClientsOnline = clientList;
            Send = send;
            Received = received;
        }

        public void prepareRespond()
        {
            Send.strMessage2 = Client.strName;
            Send.strName = Received.strMessage2;
        }

        public void ResponseToNick()
        {
            foreach (Client cInfo in ListOfClientsOnline)
            {
                if (cInfo.strName == Send.strName)
                    Response(Send.ToByte(), cInfo);
            }
        }
    }
}
