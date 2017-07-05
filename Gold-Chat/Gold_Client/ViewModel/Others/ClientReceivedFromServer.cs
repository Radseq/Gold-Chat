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
            //Task<byte[]> t = Task.Run(() => ReceiveAMessage());
            //t.ContinueWith((t1) =>
            //{
            //    t1 = Task.Run(() => ReceiveAMessage());
            //    App.Client.Buffer = t1.Result;

            //    OnDataReceived?.Invoke(this, EventArgs.Empty);
            //});
            // t.Start();
            // App.Client.Buffer = t.Result;

            //OnDataReceived?.Invoke(this, EventArgs.Empty);
            receiveDone.WaitOne();
        }

        //private async Task<byte[]> ReceiveAMessage()
        //{
        //    byte[] message = new byte[App.Client.Buffer.Length];
        //    var revcLen = await Task.Factory.FromAsync(
        //                           (callback, s) => App.Client.cSocket.BeginReceive(message, 0, message.Length, SocketFlags.None, callback, s),
        //                           ias => App.Client.cSocket.EndReceive(ias),
        //                           null);

        //    return message;
        //}

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                if (!App.Client.cSocket.Connected) return;

                Client user = (Client)ar.AsyncState;
                Socket socket = user.cSocket;
                //int bytesRead = socket.EndReceive(ar);
                //working too slow, thats why user must loggin many times
                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                //    OnDataReceived?.Invoke(this, EventArgs.Empty);
                //}));
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    OnDataReceived?.Invoke(this, EventArgs.Empty);
                }));

                //var revcLen = await Task.Factory.FromAsync(
                //         (cb, s) => socket.BeginReceive(App.Client.Buffer, 0, App.Client.Buffer.Length, SocketFlags.None, cb, s),
                //         ias => socket.EndReceive(ias),
                //         null);

                //await Task.Factory.FromAsync(ar, _ =>
                // {
                //     Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //     {
                //         OnDataReceived?.Invoke(this, EventArgs.Empty);
                //     }));
                // });
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