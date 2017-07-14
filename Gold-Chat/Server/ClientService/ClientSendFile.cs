using CommandClient;
using Server.ResponseMessages;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientSendFile : ServerResponds, IClient, IPrepareRespond
    {

        //getFileLen().ToString(), parseDirIntoFileName(), NameOfUserToSendFile, null, buffer
        List<Client> ListOfClientsOnline;

        private string fileLen;
        private string FileName;
        private string FriendName;
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
            fileLen = Received.strMessage;
            FileName = Received.strMessage2;
            FriendName = Received.strMessage3;
            fileSendByte = Received.strFileMsg;

            Send.strMessage4 = null;

            UserToSend = ClientGets.getClinetByName(ListOfClientsOnline, FriendName);

            if (UserToSend != null && fileLen != null && FileName != null)
                IsNoError = true;
            else
            {
                Send.strMessage = "No user or no file";
                Send.strFileMsg = null;
            }

        }

        public override void RespondToClient()
        {
            if (IsNoError)
            {
                SendMessageToNick sendToNick = new SendMessageToNick(ListOfClientsOnline, Send);
                sendToNick.Send.strFileMsg = fileSendByte;
                sendToNick.Send.strMessage3 = Client.strName;
                sendToNick.Send.strName = FriendName;

                sendToNick.ResponseToNick();
            }
            else
                base.RespondToClient();
        }
    }
}
