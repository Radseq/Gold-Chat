using CommandClient;
using Server.ResponseMessages;
using System;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientMessages : ServerResponds, IPrepareRespond
    {
        public event EventHandler<ClientEventArgs> ClientMessage;
        public event EventHandler<ClientEventArgs> ClientChannelMessage;

        //The collection of all clients logged into the room
        private List<Client> ClientList;

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
            ClientList = clientList;
        }

        public void Execute()
        {
            prepareResponse();
            Send.strMessage = Received.strName + ": " + Received.strMessage;
            OnClientMessage(Send.strMessage, Received.strName + ": " + Received.strMessage);
        }

        public void Response()
        {
            if (Received.strMessage2 == null)
            {
                SendMessageToAll sendToAll = new SendMessageToAll(Client, Send, ClientList);
                sendToAll.ResponseToAll();
                //base.Response();
            }
            else
            {
                string channelName = Received.strMessage2;
                SendMessageToChannel sendToChannel = new SendMessageToChannel(Send, ClientList, channelName);
                sendToChannel.ResponseToChannel();
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
