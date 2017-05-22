using CommandClient;
using Gold_Client.Model;
using System;
using System.Net.Sockets;
using System.Threading;

namespace Gold_Client.ViewModel
{
    public class ClientReceivedFromServer
    {
        public event EventHandler<ClientEventArgs> ReceiveLogExcep;

        // Singleton
        static ClientReceivedFromServer instance = null;
        static readonly object padlock = new object();

        // Singleton
        public static ClientReceivedFromServer Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new ClientReceivedFromServer();

                    return instance;
                }
            }
        }

        //public Client User
        //{
        //    get; set;
        //}

        //lets use config
        //public Configuration config = new Configuration();

        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        //public ClientReceivedFromServer()
        //{
        //    config = config.loadConfig();
        //    //User = App.Client;
        //}

        public void BeginReceive()
        {
            App.Client.cSocket.BeginReceive(App.Client.Buffer, 0, App.Client.Buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), App.Client);
            receiveDone.WaitOne();
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                //var so = (StateObject)ar.AsyncState;

                if (!App.Client.cSocket.Connected) return;

                Client user = (Client)ar.AsyncState;
                Socket socket = user.cSocket;
                socket.EndReceive(ar);

                receiveDone.Set();

                socket.BeginReceive(App.Client.Buffer, 0, App.Client.Buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), user);
            }
            catch (ObjectDisposedException ex)
            {
                //Trace.WriteLine("[Networking]::NetBase.ReceiveCallback: SocketException");
                OnReceiveLogExcep(ex.Message);
            }
            catch (Exception ex)
            {
                OnReceiveLogExcep(ex.Message);
            }
        }

        protected virtual void OnReceiveLogExcep(string message)
        {
            ReceiveLogExcep?.Invoke(this, new ClientEventArgs() { receiveLogExpceMessage = message });
        }
    }
}