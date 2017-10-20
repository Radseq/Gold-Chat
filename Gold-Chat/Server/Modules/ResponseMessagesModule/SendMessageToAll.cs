using CommandClient;
using Server.Interfaces.ResponseMessages;
using System.Collections.Generic;

namespace Server.Modules.ResponseMessagesController
{
    class SendMessageToAll : Respond, ISendMessageToAll
    {
        public void ResponseToAll(Client client, Data Send, List<Client> ListOfClientsOnline)
        {
            foreach (Client cInfo in ListOfClientsOnline)
            {
                // This user(foreach) which get ignored by someone, someone will not see when user: login,logout,msg,msg in channels
                if (!cInfo.ignoredUsers.Contains(client.strName))
                    Response(Send.ToByte(), cInfo);
            }
        }
    }
}
