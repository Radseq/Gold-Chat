using CommandClient;
using System.Collections.Generic;

namespace Server.ResponseMessages
{
    class SendMessageToNick : Respond, IServerSend
    {
        List<Client> ListOfClientsOnline;

        public Data Send { get; set; }

        public SendMessageToNick(List<Client> clientList, Data send)
        {
            ListOfClientsOnline = clientList;
            Send = send;
        }

        //public void prepareRespond()
        //{
        //    Send.strMessage2 = Client.strName;
        //    Send.strName = Received.strMessage2;
        //}

        public void ResponseToNick()
        {
            Client client = ClientGets.getClinetByName(ListOfClientsOnline, Send.strName);
            if (client != null)
                Response(Send.ToByte(), client);

        }
    }
}
