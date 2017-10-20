using CommandClient;
using System.Collections.Generic;

namespace Server.Interfaces.ResponseMessages
{
    interface ISendMessageToNick
    {
        void ResponseToNick(List<Client> ListOfClientsOnline, Data Send);
    }
}
