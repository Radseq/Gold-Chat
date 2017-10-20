using CommandClient;
using System.Collections.Generic;

namespace Server.Interfaces.ResponseMessages
{
    interface ISendMessageToSomeone
    {
        void ResponseToSomeone(List<Client> ListOfClientsOnline, Data Send, Data Received);
    }
}
