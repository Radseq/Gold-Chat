using CommandClient;
using Server.Interfaces;
using Server.Interfaces.ResponseMessages;
using Server.Modules.ResponseMessagesController;
using System;
using System.Collections.Generic;

namespace Server.Controllers
{
    class ClientMessagesController : ServerResponds, IBuildResponse
    {
        public event EventHandler<ClientEventArgs> ClientMessage;
        public event EventHandler<ClientEventArgs> ClientChannelMessage;

        private readonly ISendMessageToAll SendToClient;
        private readonly ISendMessageToChannel SendToChannel;

        public ClientMessagesController(ISendMessageToAll sendToClient, ISendMessageToChannel sendToChannel)
        {
            SendToClient = sendToClient;
            SendToChannel = sendToChannel;
        }

        private List<Client> ListOfClientsOnline;

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
            OnClientMessage(Send.strMessage, Received.strName + ": " + Received.strMessage);
        }

        public override void Response()
        {
            if (Received.strMessage2 == null)
                SendToClient.ResponseToAll(Client, Send, ListOfClientsOnline);
            else
            {
                string channelName = Received.strMessage2;
                SendToChannel.ResponseToChannel(Send, ListOfClientsOnline, channelName);
                OnClientChannelMessage(Send.strMessage, Received.strName + ": " + Received.strMessage + " On:" + channelName);
            }
        }

        protected virtual void OnClientMessage(string cMessageToSend, string cMessageRecev)
        {
            ClientMessage?.Invoke(this, new ClientEventArgs() { clientMessageToSend = cMessageToSend, clientMessageReciv = cMessageRecev });
        }

        protected virtual void OnClientChannelMessage(string cMessageToSend, string cMessageRecev)
        {
            ClientChannelMessage?.Invoke(this, new ClientEventArgs() { clientMessageToSend = cMessageToSend, clientMessageReciv = cMessageRecev });
        }
    }
}
