using CommandClient;
using Server.Interfaces;
using Server.Interfaces.ResponseMessages;
using Server.Modules.ResponseMessagesController;
using System.Collections.Generic;

namespace Server.Controllers
{
    class PrivateMessageController : ServerResponds, IBuildResponse
    {
        List<Client> ListOfClientsOnline;
        private readonly ISendMessageToNick SendPriv;

        public PrivateMessageController(ISendMessageToNick privMsg)
        {
            SendPriv = privMsg;
        }

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
            ListOfClientsOnline = clientList;
        }

        public void Execute()
        {
            prepareResponse();
            Send.strMessage = Received.strName + ": " + Received.strMessage;
        }

        public override void Response()
        {
            base.Response();

            Data send = Send;
            send.strName = Received.strMessage2;
            send.strMessage2 = Client.strName;
            SendPriv.ResponseToNick(ListOfClientsOnline, Send);
        }
    }
}