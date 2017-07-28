using CommandClient;
using Server.Interfaces;
using Server.ResponseMessages;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientSendFileInfo : ServerResponds, IClient, IBuildResponse
    {
        List<Client> ListOfClientsOnline;

        private string fileLen;
        private string FileName;
        private string FriendName;
        private Client UserToSend;

        private bool IsNoError = false;

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            ListOfClientsOnline = clientList;
            Received = receive;
            Client = client;
        }

        public void Execute()
        {
            prepareResponse();
            FriendName = Received.strMessage;
            fileLen = Received.strMessage2;
            FileName = Received.strMessage3;

            Send.strMessage4 = null;

            UserToSend = ClientGets.getClinetByName(ListOfClientsOnline, FriendName);

            if (UserToSend != null && fileLen != null && FileName != null)
                IsNoError = true;
            else if (fileLen == "AcceptReceive")
            {
                IsNoError = true;
            }
            else
            {
                Send.strMessage = "No user or no file";
                Send.strFileMsg = null;
            }

        }

        public override void Response()
        {
            if (IsNoError)
            {
                SendMessageToNick sendToNick = new SendMessageToNick(ListOfClientsOnline, Send);
                sendToNick.Send.strMessage = Client.strName;
                sendToNick.Send.strName = FriendName;

                sendToNick.ResponseToNick();
            }
            else
                base.Response();
        }
    }
}
