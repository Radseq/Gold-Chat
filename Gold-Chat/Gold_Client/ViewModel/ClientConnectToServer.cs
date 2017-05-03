using Gold_Client.Model;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Gold_Client.ViewModel
{
    class ClientConnectToServer : IClient
    {
        public Client Client
        {
            get; set;
        }

        // Singleton
        static ClientConnectToServer instance = null;
        static readonly object padlock = new object();

        // Singleton
        public static ClientConnectToServer Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new ClientConnectToServer();

                    return instance;
                }
            }
        }

        public const int PORT = 5000;
        public const string SERVER = "::1";
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(SERVER), PORT);

        private static ManualResetEvent connectDone = new ManualResetEvent(false);

        public event EventHandler<ClientEventArgs> ConnectMessage;

        public void BeginConnect()
        {
            bool part1 = Client.cSocket.Poll(1000, SelectMode.SelectRead);
            bool part2 = (Client.cSocket.Available == 0);
            if ((part1 && part2) || !Client.cSocket.Connected)
            {
                Client.cSocket.BeginConnect(ipEndPoint, new AsyncCallback(OnConnect), null);
                connectDone.WaitOne();
            }
        }

        public void ReconnectChannel()
        {
            try
            {
                Client.cSocket.Shutdown(SocketShutdown.Both);
                Client.cSocket.Disconnect(true);
                Client.cSocket.Close();
            }
            catch (Exception ex)
            {
                //ConnectivityLog.Error(ex);
                Console.WriteLine(ex);
            }
            connectDone.WaitOne();

            Client.cSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //var remoteIpAddress = IPAddress.Parse(ChannelIp);
            //ChannelEndPoint = new IPEndPoint(remoteIpAddress, ChannelPort);

            Client.cSocket.Connect(ipEndPoint);
            //Client.cSocket.BeginConnect(ipEndPoint, new AsyncCallback(OnConnect), null);
            Thread.Sleep(1000);

            if (Client.cSocket.Connected)
            {
                connectDone.Set();
            }
            else
            {

            }
        }

        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                //We are connected so we login into the server
                Client.cSocket.EndConnect(ar);

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception ex)
            {
                OnConnectExcep(ex.Message);
            }
        }

        protected virtual void OnConnectExcep(string message)
        {
            ConnectMessage?.Invoke(this, new ClientEventArgs() { connectMessage = message });
        }
    }
}
