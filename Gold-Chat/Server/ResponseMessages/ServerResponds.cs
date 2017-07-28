using CommandClient;
using Server.Interfaces;
using Server.ResponseMessages;
using System;

namespace Server
{
    class ServerResponds : Respond, IServerReceive, IServerSend, IClient
    {
        public event EventHandler<ClientEventArgs> ClientSendMessage;

        #region INTERFACE IMPLEMENTATION
        public Data Send { get; set; }

        public Data Received { get; set; }
        public Client Client { get; set; }
        #endregion INTERFACE IMPLEMENTATION

        public ServerResponds()
        {
            Send = new Data();
            Received = new Data(new byte[1024]);
        }

        public void prepareResponse()
        {
            //Send = Received; // hmm seems like Send and Received have same adress in memory
            Send.cmdCommand = Received.cmdCommand;
            Send.strMessage = Received.strMessage;
            Send.strMessage2 = Received.strMessage2;
            Send.strMessage3 = Received.strMessage3;
            Send.strMessage4 = Received.strMessage4;
            Send.strName = Received.strName;
        }

        public virtual void Response()
        {
            Response(Send.ToByte(), Client);
        }

        protected virtual void OnClientSendMessage(string cMessage) //brodcasted messages
        {
            ClientSendMessage?.Invoke(this, new ClientEventArgs() { clientMessageToSend = cMessage }); //TODO all Send message
        }
    }
}
