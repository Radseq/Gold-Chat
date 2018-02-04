using CommandClient;
using Gold_Client.Model;
using System;
using System.Net.Sockets;
using System.Security.Cryptography;
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

        private ClientSendToServer()
        {
            User = App.Client;
            if (User.cSocket == null)
                User.cSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
        }

        public event EventHandler<ClientEventArgs> SendException;

        public Client User
        {
            get;
            set;
        }

        private void OnSend(IAsyncResult ar)
        {
            try
            {
                User.cSocket.EndSend(ar);
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

        public void SendToServer(Command command, string strMessage = null, string strMessage2 = null, string strMessage3 = null, string strMessage4 = null, byte[] fileByte = null)
        {
            Data msgToSend = new Data();
            msgToSend.cmdCommand = command;
            msgToSend.strName = User.strName;
            msgToSend.strMessage = strMessage;
            msgToSend.strMessage2 = strMessage2;
            msgToSend.strMessage3 = strMessage3;
            msgToSend.strMessage4 = strMessage4;
            msgToSend.strFileMsg = fileByte;

            byte[] toSendByteData = new byte[1048576];
            toSendByteData = msgToSend.ToByte();

            if (!User.cSocket.Connected && msgToSend.cmdCommand != Command.Logout)
            {
                ClientConnectToServer clientConnectToServer = ClientConnectToServer.Instance;
                clientConnectToServer.BeginConnect();
                BeginSend(toSendByteData);
            }
            else if (msgToSend.cmdCommand == Command.Logout)
                Send(toSendByteData);
            else BeginSend(toSendByteData);
        }

        private void BeginSend(byte[] byteData)
        {
            User.cSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
            sendDone.WaitOne();
        }

        private void Send(byte[] byteData)
        {
            User.cSocket.Send(byteData, 0, byteData.Length, SocketFlags.None);
        }

        public void Logout()
        {
            // Release the socket.
            User.cSocket.Shutdown(SocketShutdown.Both);
            User.cSocket.Close();
            User.cSocket.Dispose();
        }

        protected virtual void OnSendExcep(string message)
        {
            SendException?.Invoke(this, new ClientEventArgs() { sendExcepMessage = message });
        }

        //md5
        public string CalculateChecksum(string inputString)
        {
            var md5 = new MD5CryptoServiceProvider();
            var hashbytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(inputString));
            var hashstring = "";
            foreach (var hashbyte in hashbytes)
                hashstring += hashbyte.ToString("x2");

            return hashstring;
        }
    }
}
