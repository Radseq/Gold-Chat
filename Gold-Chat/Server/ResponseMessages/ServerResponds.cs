using CommandClient;
using Server.ResponseMessages;
using System;

namespace Server
{
    // Todo class to send 
    class ServerResponds : Respond, IServerReceive, IServerSend, IClient
    {
        public event EventHandler<ClientEventArgs> ClientSendMessage;

        //protected List<Client> clientList = new List<Client>();
        //private Data msgToSend;
        //private Data msgReceived;

        #region INTERFACE IMPLEMENTATION
        public Data Send { get; set; }

        public Data Received { get; set; }
        public Client Client { get; set; }

        //public Client Client { get; set; }

        #endregion INTERFACE IMPLEMENTATION

        //protected List<Client> clientList { get; set; }

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

        public virtual void RespondToClient()
        {
            Response(Send.ToByte(), Client);
        }

        protected virtual void OnClientSendMessage(string cMessage) //brodcasted messages
        {
            ClientSendMessage?.Invoke(this, new ClientEventArgs() { clientMessageToSend = cMessage });// do zrobienia cale data a nie tylko msgMessage
        }
    }
}
