using Gold_Client.Model;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

namespace Gold_Client.ViewModel
{
    public class ClientReceivedFromServer
    {
        public event EventHandler<ClientEventArgs> ReceiveLogExcep;

        public event EventHandler OnDataReceived;

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
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        public void BeginReceive()
        {
            App.Client.cSocket.BeginReceive(App.Client.Buffer, 0, App.Client.Buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), App.Client);
            receiveDone.WaitOne();
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                if (!App.Client.cSocket.Connected) return;

                Client user = (Client)ar.AsyncState;
                Socket socket = user.cSocket;
                int bytesRead = socket.EndReceive(ar);
                //working too slow, thats why user must loggin many times
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    OnDataReceived?.Invoke(this, EventArgs.Empty);
                }));

                socket.BeginReceive(App.Client.Buffer, 0, App.Client.Buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), user);
                receiveDone.Set();
            }
            catch (ObjectDisposedException ex)
            {
                OnReceiveLogExcep(ex.Message);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("[Networking]::ClientReceivedFromServer.ReceiveCallback:" + ex);
                OnReceiveLogExcep(ex.Message);
            }
        }

        protected virtual void OnReceiveLogExcep(string message)
        {
            ReceiveLogExcep?.Invoke(this, new ClientEventArgs() { receiveLogExpceMessage = message });
        }
    }
}