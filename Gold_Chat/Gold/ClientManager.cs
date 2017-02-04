using CommandClient;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;

namespace Gold
{
    public class ClientEventArgs : EventArgs
    {
        public string connectMessage { get; set; }
        public string sendExcepMessage { get; set; }
        public string clientLoginMessage { get; set; }
        public string clientLoginName { get; set; } // using when new client login to room and other users need add this name to userlist + refresh userlist
        public bool clientLoginSucces { get; set; }
        public Socket clientSocket { get; set; }
        public string clientRegMessage { get; set; }
        public string clientReSendEmailMessage { get; set; }
        public string receiveLogExpceMessage { get; set; }

        //or just give this object public ServerManager ServerManager { get; set; }
        //For Main program
        public string clientLogoutMessage { get; set; }
        public string clientListMessage { get; set; }
        public string clientMessage { get; set; }
        public string clientPrivMessage { get; set; }
        public string clientFriendName { get; set; }

        public int clientPingTime { get; set; }
        public string clientPingMessage { get; set; }
        public string clientChangePassMessage { get; set; }
    }

    public class ClientManager
    {
        //For login
        public event EventHandler<ClientEventArgs> ClientRegistration;
        public event EventHandler<ClientEventArgs> ClientReSendEmail;
        public event EventHandler<ClientEventArgs> ReceiveLogExcep;
        public event EventHandler<ClientEventArgs> ConnectMessage;
        public event EventHandler<ClientEventArgs> SendException;
        public event EventHandler<ClientEventArgs> ClientLogin;
        public event EventHandler<ClientEventArgs> ClientSuccesLogin;
        //For Main program
        public event EventHandler<ClientEventArgs> ClientLogout;
        public event EventHandler<ClientEventArgs> ClientList;
        public event EventHandler<ClientEventArgs> ClientMessage;
        public event EventHandler<ClientEventArgs> ClientPrivMessage;
        //ping
        public event EventHandler<ClientEventArgs> ClientPing;

        public event EventHandler<ClientEventArgs> ClientChangePass;

        Socket socket;
        //object msgS;
        //public string clientPassword;
        public string userName;

        private System.Timers.Timer pingTimer;
        //private System.Timers.Timer messageTimer;

        public const int PORT = 5000;
        public const string SERVER = "::1";

        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(SERVER), PORT);

        private byte[] byteData = new byte[1024];

        //lets use config
        public Configuration config = new Configuration();

        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        public ClientManager()
        {
            config = config.loadConfig();

            socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

            //pingTimer = new System.Timers.Timer();
            //pingTimer.Interval = 5000;
            //pingTimer.Elapsed += new ElapsedEventHandler(this.pingServer);
            //pingTimer.Start();

            //messageTimer = new System.Timers.Timer();
            //messageTimer.Interval = 1000;
            //messageTimer.Elapsed += new ElapsedEventHandler(this.pingServer);
            //messageTimer.Start();
        }

        private void pingServer(object sender, EventArgs e)
        {
            Ping pinger = new Ping();
            try
            {
                PingReply reply = pinger.Send("127.0.0.1");
                if (reply.Status == IPStatus.Success)
                    OnClientPing((int)reply.RoundtripTime, "Server Online");
                OnClientPing(0, "Server Offline");

            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
        }

        public void BeginConnect(/*string name, string password*/)
        {
            //userName = name;
            //clientPassword = password;

            bool part1 = socket.Poll(1000, SelectMode.SelectRead);
            bool part2 = (socket.Available == 0);
            if ((part1 && part2) || !socket.Connected)
            {
                //BeginConnect();
                socket.BeginConnect(ipEndPoint, new AsyncCallback(OnConnect), null);
                connectDone.WaitOne();
            }
        }

        private void OnSend(IAsyncResult ar)
        {
            try
            {
                socket.EndSend(ar);

                // Retrieve the socket from the state object.
                //Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                //int bytesSent = client.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
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
        // to check
        public void BeginSend(byte[] byteData)
        {
            //messageTimer.Elapsed += new ElapsedEventHandler((s, e) =>
            socket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
            sendDone.WaitOne();
        }

        public void LogoutSend(byte[] byteData)
        {
            socket.Send(byteData, 0, byteData.Length, SocketFlags.None);
            receiveDone.WaitOne();//must be, case we must wait till receive ends
            // Release the socket.
            //socket.Shutdown(SocketShutdown.Both);
            //socket.Close();
        }

        public void BeginReceive()
        {
            byteData = new byte[1024];
            socket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
            receiveDone.WaitOne();
        }

        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                //We are connected so we login into the server
                socket.EndConnect(ar);

                // Signal that the connection has been made.
                connectDone.Set();

                //Start listening to the data asynchronously
                BeginReceive();

                /*total wasted time here 5h, cuse 

                byteData = new byte[1024];
                    //Start listening to the data asynchronously
                    clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);

                MUST BE AFTER ON CONNECT, OnReceive wont get full message
                or use connectDone.Set();
                */
                //}));
            }
            catch (Exception ex)
            {
                OnConnectExcep(ex.Message);
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                //var so = (StateObject)ar.AsyncState;

                //if (!socket.Connected) return;

                socket.EndReceive(ar);

                Data msgReceived = new Data(byteData);
                //Accordingly process the message received
                switch (msgReceived.cmdCommand)
                {
                    case Command.Login:
                        //if (msgReceived.strName == userName)
                        OnClientLogin(msgReceived.strMessage, msgReceived.strName);
                        if (msgReceived.strName == userName && msgReceived.strMessage == "You are succesfully Log in") // && msgReceived.loginName != userName
                            OnClientSuccesLogin(true, socket);
                        break;

                    case Command.Reg:
                        OnClientRegister(msgReceived.strMessage);
                        break;

                    case Command.changePassword:
                        OnClientChangePass(msgReceived.strMessage);
                        break;

                    case Command.ReSendEmail:
                        OnClientReSendEmail(msgReceived.strMessage);
                        break;

                    case Command.Logout:
                        OnClientLogout(msgReceived.strName);
                        break;

                    case Command.Message:
                        break;

                    case Command.privMessage:
                        break;

                    case Command.List:
                        OnClientList(msgReceived.strMessage);
                        break;
                }
                // Procedure listening for server messages.
                if (msgReceived.strMessage != null && msgReceived.cmdCommand != Command.List && msgReceived.cmdCommand != Command.privMessage)
                    OnClientMessage(msgReceived.strMessage + "\r\n");
                else if (msgReceived.strMessage != null && msgReceived.cmdCommand == Command.privMessage && msgReceived.cmdCommand != Command.List && msgReceived.strMessage != null)
                {
                    OnClientPrivMessage(msgReceived.strMessage + "\r\n", msgReceived.strName);
                }

                byteData = new byte[1024];
                receiveDone.Set();
                socket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
                //receiveDone.Set();
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

        protected virtual void OnConnectExcep(string message)
        {
            ConnectMessage?.Invoke(this, new ClientEventArgs() { connectMessage = message });
        }

        protected virtual void OnSendExcep(string message)
        {
            SendException?.Invoke(this, new ClientEventArgs() { sendExcepMessage = message });
        }

        protected virtual void OnReceiveLogExcep(string message)
        {
            ReceiveLogExcep?.Invoke(this, new ClientEventArgs() { receiveLogExpceMessage = message });
        }

        protected virtual void OnClientLogin(string Message, string userName)
        {
            ClientLogin?.Invoke(this, new ClientEventArgs() { clientLoginMessage = Message, clientLoginName = userName });
        }
        //if client succesfully login
        protected virtual void OnClientSuccesLogin(bool isSucces, Socket soc)
        {
            ClientSuccesLogin?.Invoke(this, new ClientEventArgs() { clientLoginSucces = isSucces, clientSocket = soc });
        }

        protected virtual void OnClientRegister(string ReceiveMessage)
        {
            ClientRegistration?.Invoke(this, new ClientEventArgs() { clientRegMessage = ReceiveMessage });
        }

        protected virtual void OnClientReSendEmail(string ReSendEmailMessage)
        {
            ClientReSendEmail?.Invoke(this, new ClientEventArgs() { clientReSendEmailMessage = ReSendEmailMessage });
        }

        //For Main program
        protected virtual void OnClientLogout(string Message)
        {
            ClientLogout?.Invoke(this, new ClientEventArgs() { clientLogoutMessage = Message });
        }

        protected virtual void OnClientList(string Message)
        {
            ClientList?.Invoke(this, new ClientEventArgs() { clientListMessage = Message });
        }

        protected virtual void OnClientMessage(string Message)
        {
            ClientMessage?.Invoke(this, new ClientEventArgs() { clientMessage = Message });
        }

        protected virtual void OnClientPrivMessage(string Message, string friendName)
        {
            ClientPrivMessage?.Invoke(this, new ClientEventArgs() { clientPrivMessage = Message, clientFriendName = friendName });
        }

        protected virtual void OnClientPing(int time, string message)
        {
            ClientPing?.Invoke(this, new ClientEventArgs() { clientPingTime = time, clientPingMessage = message });
        }

        protected virtual void OnClientChangePass(string message)
        {
            ClientChangePass?.Invoke(this, new ClientEventArgs() { clientChangePassMessage = message });
        }
    }
}
