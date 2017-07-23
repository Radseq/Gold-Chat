using CommandClient;
using Server.Interfaces;
using Server.ResponseMessages;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientSendFile : ServerResponds, IClient, IBuildResponse
    {

        //getFileLen().ToString(), parseDirIntoFileName(), NameOfUserToSendFile, null, buffer
        List<Client> ListOfClientsOnline;

        private string FriendName;
        private string FileName;
        private Client UserToSend;
        private byte[] fileSendByte;
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
            FileName = Received.strMessage2;
            fileSendByte = Received.strFileMsg;

            UserToSend = ClientGets.getClinetByName(ListOfClientsOnline, FriendName);

            if (UserToSend != null && FileName != null)
                IsNoError = true;
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
                sendToNick.Send.strFileMsg = fileSendByte;
                sendToNick.Send.strMessage = Client.strName;
                sendToNick.Send.strName = FriendName;

                sendToNick.ResponseToNick();
            }
            else
                base.Response();
        }
    }
}
