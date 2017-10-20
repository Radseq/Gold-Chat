using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Gold_Chat_Core.SocketWorking
{
    class ClientConnectToServer : IClient
    {
        public Client User
        {
            get; set;
        }

        public event EventHandler<ClientEventArgs> ConnectMessage;

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
        IPEndPoint ServerIpEndPoint = new IPEndPoint(IPAddress.Parse(SERVER), PORT);

        private static ManualResetEvent connectDone = new ManualResetEvent(false);

        private ClientConnectToServer()
        {
            User = App.Client;
        }

        public void BeginConnect()
        {
            bool part1 = User.cSocket.Poll(1000, SelectMode.SelectRead);
            bool part2 = (User.cSocket.Available == 0);
            if ((part1 && part2) || !User.cSocket.Connected)
            {
                User.cSocket.BeginConnect(ServerIpEndPoint, new AsyncCallback(OnConnect), null);
                connectDone.WaitOne();
            }
        }

        public void ReconnectChannel()
        {
            try
            {
                User.cSocket.Shutdown(SocketShutdown.Both);
                User.cSocket.Disconnect(true);
                User.cSocket.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            connectDone.WaitOne();

            User.cSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

            User.cSocket.Connect(ServerIpEndPoint);
            //Client.cSocket.BeginConnect(ipEndPoint, new AsyncCallback(OnConnect), null);
            Thread.Sleep(1000);

            if (User.cSocket.Connected)
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
                User.cSocket.EndConnect(ar);
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
