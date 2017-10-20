using CommandClient;
using Server.Interfaces;
using Server.Interfaces.ResponseMessages;
using Server.Modules.ResponseMessagesController;
using Server.Utilies;
using System.Collections.Generic;

namespace Server.Controllers
{
    class ClientKickController : ServerResponds, IBuildResponse
    {
        List<Client> ListOfClientsOnline;

        private string KickReason;
        private string KickedUserName;
        private Client KickeUser;

        private readonly ISendMessageToAll SendToAll;

        public ClientKickController(ISendMessageToAll sendToAll)
        {
            SendToAll = sendToAll;
        }

        bool isKicked = false;

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
            ListOfClientsOnline = clientList;
        }

        public void Execute()
        {
            prepareResponse();
            KickedUserName = Received.strMessage;   // Nick of kicked user
            KickReason = Received.strMessage2;      // Reason of kick

            KickeUser = ClientGets.getClinetByName(ListOfClientsOnline, KickedUserName);

            if (KickeUser != null)
                if (Client.permission > 0)
                {
                    if (KickeUser.permission == 0)
                    {
                        isKicked = true;
                        Send.strMessage2 = " kicked Reason: " + KickReason;
                    }
                    else Send.strMessage2 = "You cannot kick " + KickedUserName + " because client is admin or there is no user with this name";

                }
                else Send.strMessage2 = "You cannot kick " + KickedUserName + " because you dont have permissions";
        }

        public override void Response()
        {
            if (isKicked)
            {
                SendToAll.ResponseToAll(Client, Send, ListOfClientsOnline); //ignored wont get this msg
                KickeUser.cSocket.Close();
            }
            else
                base.Response();
        }
    }
}
