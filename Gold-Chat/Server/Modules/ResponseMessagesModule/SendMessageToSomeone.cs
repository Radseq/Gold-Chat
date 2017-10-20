using CommandClient;
using Server.Interfaces.ResponseMessages;
using System.Collections.Generic;

namespace Server.Modules.ResponseMessagesController
{
    class SendMessageToSomeone : Respond, ISendMessageToSomeone
    {
        public void ResponseToSomeone(List<Client> ListOfClientsOnline, Data Send, Data Received)
        {
            foreach (Client cInfo in ListOfClientsOnline)
            {
                if (cInfo.strName == Received.strMessage2 || Received.strName == cInfo.strName)
                    Response(Send.ToByte(), cInfo);
            }
        }
    }
}
