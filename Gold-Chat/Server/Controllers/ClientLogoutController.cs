using CommandClient;
using Server.Interfaces;
using Server.Interfaces.ResponseMessages;
using Server.Modules.ResponseMessagesController;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Server.ClientService
{
    class ClientLogoutController : ServerResponds, IBuildResponse
    {
        //The collection of all clients logged into the room
        private List<Client> ListOfClientsOnline;
        //list of all channels
        private List<Channel> ChannelsList;
        private readonly ISendMessageToAll SendToAll;

        public event EventHandler<ClientEventArgs> ClientLogoutEvent;

        public ClientLogoutController(ISendMessageToAll sendToAll)
        {
            SendToAll = sendToAll;
        }

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
            ChannelsList = channelList;
            ListOfClientsOnline = clientList;
        }

        public void Execute()
        {
            prepareResponse();

            // When a user wants to log out of the server then we search for him 
            // in the list of clients and close the corresponding connection
            foreach (Client clientInList in ListOfClientsOnline)
            {
                if (Client.cSocket == clientInList.cSocket)
                {
                    ListOfClientsOnline.Remove(clientInList);
                    break;
                }
            }

            foreach (Channel ch in ChannelsList) // Dont need to send list etc to user because all channels got msg that user log out and channel check if exists in theirs list
            {
                if (ch.Users.Contains(Client.strName))
                    ch.Users.Remove(Client.strName);
            }

            Send.strMessage = Client.strName;
            OnClientLogout(Client.strName, Client.cSocket);

            Client.cSocket.Close();


            Send.cmdCommand = Command.Logout;
            Send.strName = Client.strName;
        }

        public override void Response()
        {
            SendToAll.ResponseToAll(Client, Send, ListOfClientsOnline);

            OnClientSendMessage(Send.strMessage);
        }

        protected virtual void OnClientLogout(string cName, Socket socket)
        {
            ClientLogoutEvent?.Invoke(this, new ClientEventArgs() { clientName = cName, clientSocket = socket });
        }
    }
}
