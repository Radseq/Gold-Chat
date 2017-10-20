using CommandClient;
using Server.Interfaces.ResponseMessages;
using Server.Utilies;
using System.Collections.Generic;

namespace Server.Modules.ResponseMessagesController
{
    class SendMessageToNick : Respond, ISendMessageToNick
    {
        public void ResponseToNick(List<Client> ListOfClientsOnline, Data Send)
        {
            Client client = ClientGets.getClinetByName(ListOfClientsOnline, Send.strName);
            if (client != null)
                Response(Send.ToByte(), client);

        }
    }
}
