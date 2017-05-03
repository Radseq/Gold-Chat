using System;
using System.Net.Sockets;

namespace Server.ResponseMessages
{
    class Respond
    {
        protected virtual void Response(byte[] message, Client client)
        {
            client.cSocket.BeginSend(message, 0, message.Length, SocketFlags.None, new AsyncCallback(OnSend), client.cSocket);
        }

        protected virtual void OnSend(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndSend(ar);
            }
            catch (Exception ex)
            {
                LoggerToFile servLogger = LoggerToFile.Instance;
                servLogger.Log(ex.Message);
            }
        }
    }
}
