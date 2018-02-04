namespace Gold_Client.ViewModel
{
    public class ClientReceivedFromServer
    {
        ////public event EventHandler<ClientEventArgs> ReceiveLogExcep;

        //public event EventHandler OnDataReceived;
        //private readonly ManualResetEvent received = new ManualResetEvent(false);

        //// Singleton
        //static ClientReceivedFromServer instance = null;
        //static readonly object padlock = new object();

        //public bool IsClientStartReceive { get; set; }

        //// Singleton
        //public static ClientReceivedFromServer Instance
        //{
        //    get
        //    {
        //        lock (padlock)
        //        {
        //            if (instance == null)
        //                instance = new ClientReceivedFromServer();

        //            return instance;
        //        }
        //    }
        //}

        //public void Receive()
        //{
        //    App.Client.cSocket.BeginReceive(App.Client.Buffer, 0, App.Client.Buffer.Length, SocketFlags.None, ReceiveCallback, App.Client);
        //    IsClientStartReceive = true;
        //    received.WaitOne();
        //}

        //private void ReceiveCallback(IAsyncResult result)
        //{
        //    if (!App.Client.cSocket.Connected) return;

        //    Client user = (Client)result.AsyncState;
        //    Socket socket = user.cSocket;
        //    int bytesRead = socket.EndReceive(result);

        //    if (bytesRead > 0)
        //    {
        //        user.cSocket.BeginReceive(App.Client.Buffer, 0, App.Client.Buffer.Length, SocketFlags.None, ReceiveCallback, user);
        //    }

        //    if (bytesRead == App.Client.Buffer.Length)
        //    {
        //        user.cSocket.BeginReceive(App.Client.Buffer, 0, App.Client.Buffer.Length, SocketFlags.None, ReceiveCallback, user);
        //    }
        //    else
        //    {
        //        received.Set();
        //        OnDataReceived?.Invoke(null, EventArgs.Empty);
        //    }
        //}

        //protected virtual void OnReceiveLogExcep(string message)
        //{
        //    //ReceiveLogExcep?.Invoke(this, new ClientEventArgs() { receiveLogExpceMessage = message });
        //}
    }
}