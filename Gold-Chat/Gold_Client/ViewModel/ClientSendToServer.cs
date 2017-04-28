using CommandClient;
using Gold_Client.Model;
using System;
using System.Net.Sockets;
using System.Threading;

namespace Gold_Client.ViewModel
{
    class ClientSendToServer : IClient
    {
        // Singleton
        static ClientSendToServer instance = null;
        static readonly object padlock = new object();

        // Singleton
        public static ClientSendToServer Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new ClientSendToServer();

                    return instance;
                }
            }
        }

        private static ManualResetEvent sendDone = new ManualResetEvent(false);

        public event EventHandler<ClientEventArgs> SendException;

        public Client Client
        {
            get; set;
        }

        private void OnSend(IAsyncResult ar)
        {
            try
            {
                Client.cSocket.EndSend(ar);
                sendDone.Set();
            }
            catch (ObjectDisposedException ode)
            {
                OnSendExcep(ode.Message);
            }
            catch (Exception ex)
            {
                OnSendExcep(ex.Message);
            }
        }

        public void SendToServer(Command command, string strMessage = null, string strMessage2 = null, string strMessage3 = null, string strMessage4 = null)
        {
            Data msgToSend = new Data();
            msgToSend.cmdCommand = command;
            msgToSend.strName = Client.strName;
            msgToSend.strMessage = strMessage;
            msgToSend.strMessage2 = strMessage2;
            msgToSend.strMessage3 = strMessage3;
            msgToSend.strMessage4 = strMessage4;

            byte[] toSendByteData = new byte[1024];
            toSendByteData = msgToSend.ToByte();

            if (Client.cSocket.Connected)
            {
                ClientConnectToServer clientConnectToServer = new ClientConnectToServer();
                clientConnectToServer.BeginConnect();
                BeginSend(toSendByteData);
            }
        }

        private void BeginSend(byte[] byteData)
        {
            Client.cSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
            sendDone.WaitOne();
        }

        public void LogoutSend()
        {
            //Send a message to logout of the server
            Data msgToSend = new Data();
            msgToSend.cmdCommand = Command.Logout;
            msgToSend.strName = Client.strName;

            byte[] logoutMessage = msgToSend.ToByte();

            Client.cSocket.Send(logoutMessage, 0, logoutMessage.Length, SocketFlags.None);
            // Release the socket.
            Client.cSocket.Shutdown(SocketShutdown.Both);
            Client.cSocket.Close();
            Client.cSocket.Dispose();
        }

        protected virtual void OnSendExcep(string message)
        {
            SendException?.Invoke(this, new ClientEventArgs() { sendExcepMessage = message });
        }

    }
}
