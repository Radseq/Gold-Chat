using CommandClient;
using System.Collections.Generic;

namespace Server.ClientService.List
{
    class SendOnlineListOfClients : ClientListManager
    {
        public SendOnlineListOfClients(List<Client> clientList, Data send)
        {
            ListOfClientsOnline = clientList;
            Send = send;
        }

        /// <summary>
        /// Sending list of logged users 
        /// adam*bob*matty
        /// </summary>
        /// <param name="conClient">Connected User to server</param>
        /// <param name="msgToSend">Respond to Client</param>
        public new void Execute()
        {
            //prepareResponse();
            Send.cmdCommand = Command.List;
            Send.strName = null;
            Send.strMessage = null;

            //Collect the names of the user in the chat room
            foreach (Client client in ListOfClientsOnline)
            {
                //To keep things simple we use asterisk as the marker to separate the user names
                Send.strMessage2 += client.strName + "*";
            }
            OnClientList(Send.strMessage2, ""); //second parameter used for diffrent lists
        }
    }
}
