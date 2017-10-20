using CommandClient;
using Server.Interfaces;
using Server.Interfaces.ResponseMessages;
using Server.Modules.ResponseMessagesController;
using Server.Utilies;
using System.Collections.Generic;

namespace Server.Controllers
{
    class SendFileInfoController : ServerResponds, IClient, IBuildResponse
    {
        List<Client> ListOfClientsOnline;

        private string fileLen;
        private string FileName;
        private string FriendName;
        private Client UserToSend;

        private readonly ISendMessageToNick ResponseToNick;

        private bool IsNoError = false;

        public SendFileInfoController(ISendMessageToNick responseToNick)
        {
            ResponseToNick = responseToNick;
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
                Data send = Send;
                send.strMessage = Client.strName;
                send.strName = FriendName;
                ResponseToNick.ResponseToNick(ListOfClientsOnline, send);
            }
            else
                base.Response();
        }
    }
}
