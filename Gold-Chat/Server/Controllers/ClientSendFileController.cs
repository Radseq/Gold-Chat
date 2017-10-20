using CommandClient;
using Server.Interfaces;
using Server.Interfaces.ResponseMessages;
using Server.Modules.ResponseMessagesController;
using Server.Utilies;
using System.Collections.Generic;

namespace Server.Controllers
{
    class ClientSendFileController : ServerResponds, IClient, IBuildResponse
    {

        //getFileLen().ToString(), parseDirIntoFileName(), NameOfUserToSendFile, null, buffer
        List<Client> ListOfClientsOnline;

        private string FriendName;
        private string FileName;
        private Client UserToSend;
        private byte[] fileSendByte;
        private bool IsNoError = false;

        private readonly ISendMessageToNick ToNick;

        public ClientSendFileController(ISendMessageToNick toNick)
        {
            ToNick = toNick;
        }

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
                Data send = Send;
                send.strFileMsg = fileSendByte;
                send.strMessage = Client.strName;
                send.strName = FriendName;
                ToNick.ResponseToNick(ListOfClientsOnline, send);
            }
            else
                base.Response();
        }
    }
}
