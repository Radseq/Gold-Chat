using System;
using System.Collections.Generic;
using System.Net.Sockets;
using CommandClient;

namespace Server.ResponseMessages
{
    class SendMessageToSomeone : Respond, IServerSend, IServerReceive
    {
        List<Client> ClientList;

        #region INTERFACE IMPLEMENTATION
        public Data Send { get; set; }
        public Data Received { get; set; }
        #endregion INTERFACE IMPLEMENTATION

        public SendMessageToSomeone(List<Client> clientList, Data send)
        {
            Send = send;
            ClientList = clientList;
        }

        //public void prepareResponse()
        //{
        //    Send = Received;
        //}

        public void ResponseToSomeone()
        {
            foreach (Client cInfo in ClientList)
            {
                if (cInfo.strName == Received.strMessage2 || Received.strName == cInfo.strName)
                    Response(Send.ToByte(), cInfo);
            }
        }
    }
}
