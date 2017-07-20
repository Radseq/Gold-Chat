using CommandClient;
using System;
using System.Net;
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
        //for channel
        public string clientChannelMsg { get; set; }
        public string clientChannelMsg2 { get; set; }
        public string clientListChannelsMessage { get; set; }
        public string clientChannelMessage { get; set; }
        public string clientChannelName { get; set; }

        public string clientName { get; set; } //for friend or i should use clientLoginName
        //friends
        public string clientListFriendsMessage { get; set; }
        //Ignore
        public string clientIgnoreOption { get; set; }
        public string clientIgnoreMessage { get; set; }
        public string clientIgnoreName { get; set; }
        //kick/ban
        public string clientBanReason { get; set; }
        public string clientBanTime { get; set; }
        public string clientKickReason { get; set; }
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
        public event EventHandler<ClientEventArgs> ClientLostPass;

        //channel
        public event EventHandler<ClientEventArgs> ClientCreateChannel;
        public event EventHandler<ClientEventArgs> ClientJoinChannel;
        public event EventHandler<ClientEventArgs> ClientDeleteChannel;
        public event EventHandler<ClientEventArgs> ClientExitChannel; //exit
        public event EventHandler<ClientEventArgs> ClientEditChannel; //edit
        public event EventHandler<ClientEventArgs> ClientListChannel;
        public event EventHandler<ClientEventArgs> ClientListChannelJoined;
        public event EventHandler<ClientEventArgs> ClientListChannelUsers;
        public event EventHandler<ClientEventArgs> ClientChannelMessage;
        public event EventHandler<ClientEventArgs> ClientChannelEnter;
        public event EventHandler<ClientEventArgs> ClientChannelEnterDeny;
        public event EventHandler<ClientEventArgs> ClientChannelLeave;

        //friend
        public event EventHandler<ClientEventArgs> ClientAddFriend;
        public event EventHandler<ClientEventArgs> ClientAcceptFriend;
        public event EventHandler<ClientEventArgs> ClientDeleteFriend;
        public event EventHandler<ClientEventArgs> ClientDenyFriend;
        public event EventHandler<ClientEventArgs> ClientListFriends;
        //ignore
        public event EventHandler<ClientEventArgs> ClientIgnoreUser;
        public event EventHandler<ClientEventArgs> ClientDeleteIgnoredUser;
        public event EventHandler<ClientEventArgs> ClientListIgnored;
        //kick/ban
        public event EventHandler<ClientEventArgs> ClientKickFromChannel;
        public event EventHandler<ClientEventArgs> ClientKickFromSerwer;
        public event EventHandler<ClientEventArgs> ClientBanFromSerwer;
        //public event EventHandler<ClientEventArgs> ClientListIgnored;

        Socket socket;
        //string userName;

        //private System.Timers.Timer pingTimer;
        //private System.Timers.Timer messageTimer;

        bool serverError = false;
        bool IsClientConnectedToServer { get; set; }

        public int permission { get; set; }

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
            IsClientConnectedToServer = false;
            //pingTimer = new System.Timers.Timer();
            //pingTimer.Interval = 5000;
            //pingTimer.Elapsed += new ElapsedEventHandler(this.pingServer);
            //pingTimer.Start();

            //messageTimer = new System.Timers.Timer();
            //messageTimer.Interval = 1000;
            //messageTimer.Elapsed += new ElapsedEventHandler(this.pingServer);
            //messageTimer.Start();
        }

        //private void pingServer(object sender, EventArgs e)
        //{
        //    Ping pinger = new Ping();
        //    try
        //    {
        //        PingReply reply = pinger.Send("127.0.0.1");
        //        if (reply.Status == IPStatus.Success)
        //            OnClientPing((int)reply.RoundtripTime, "Server Online");
        //        OnClientPing(0, "Server Offline");

        //    }
        //    catch (PingException)
        //    {
        //        // Discard PingExceptions and return false;
        //    }
        //}

        private void BeginConnect(/*string name, string password*/)
        {
            //userName = name;
            //clientPassword = password;

            bool part1 = socket.Poll(1000, SelectMode.SelectRead);
            bool part2 = (socket.Available == 0);
            if ((part1 && part2) || !socket.Connected)
            {
                IsClientConnectedToServer = true;
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

        public void SendToServer(Command command, string strMessage = null, string strMessage2 = null, string strMessage3 = null, string strMessage4 = null)
        {
            if (!IsClientConnectedToServer)
                BeginConnect();

            Data msgToSend = new Data();
            msgToSend.cmdCommand = command;
            msgToSend.strName = App.clientName;
            msgToSend.strMessage = strMessage;
            msgToSend.strMessage2 = strMessage2;
            msgToSend.strMessage3 = strMessage3;
            msgToSend.strMessage4 = strMessage4;

            byte[] toSendByteData = new byte[1024];
            toSendByteData = msgToSend.ToByte();

            if (IsClientConnectedToServer)
                BeginSend(toSendByteData);
        }

        private void BeginSend(byte[] byteData)
        {
            //messageTimer.Elapsed += new ElapsedEventHandler((s, e) =>
            socket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
            sendDone.WaitOne();
        }

        public void LogoutSend()
        {
            //Send a message to logout of the server
            Data msgToSend = new Data();
            msgToSend.cmdCommand = Command.Logout;
            msgToSend.strName = App.clientName;

            byte[] logoutMessage = msgToSend.ToByte();

            IsClientConnectedToServer = false;
            socket.Send(logoutMessage, 0, logoutMessage.Length, SocketFlags.None);
            // Release the socket.
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            socket.Dispose();
        }

        private void BeginReceive()
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
            }
            catch (Exception ex)
            {
                OnConnectExcep(ex.Message);
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            if (!IsClientConnectedToServer || serverError == true)
                return;

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
                        if (msgReceived.strName == App.clientName && msgReceived.strMessage == "You are succesfully Log in") // && msgReceived.loginName != userName
                        {
                            OnClientSuccesLogin(true, socket);
                            permission = int.Parse(msgReceived.strMessage2);
                        }
                        else if (msgReceived.strMessage == null)
                            serverError = true;
                        else OnClientLogin(msgReceived.strMessage, msgReceived.strName); //someone other login, use to add user to as list etc.
                        break;

                    case Command.Registration:
                        OnClientRegister(msgReceived.strMessage);
                        break;

                    case Command.changePassword:
                        OnClientChangePass(msgReceived.strMessage);
                        break;

                    case Command.lostPassword:
                        OnClientLostPassword(msgReceived.strMessage, msgReceived.strMessage2);
                        break;

                    case Command.SendActivationCode:
                        OnClientReSendEmail(msgReceived.strMessage);
                        break;

                    case Command.Logout:
                        OnClientLogout(msgReceived.strName);
                        break;

                    case Command.Message:
                        if (msgReceived.strMessage2 == null) //strMessage2 -> ChannelName
                            OnClientMessage(msgReceived.strMessage + "\r\n");
                        else
                            OnClientChannelMessage(msgReceived.strMessage + "\r\n"); //strMessage -> ChannelMessage
                        break;

                    case Command.privMessage:
                        OnClientPrivMessage(msgReceived.strMessage + "\r\n", msgReceived.strMessage2);
                        break;

                    case Command.createChannel:
                        OnClientCreateChannel(msgReceived.strMessage, msgReceived.strMessage2, msgReceived.strName);
                        break;

                    case Command.joinChannel:
                        OnClientJoinChannel(msgReceived.strMessage, msgReceived.strMessage2, msgReceived.strMessage3);
                        break;

                    case Command.exitChannel:
                        OnClientExitChannel(msgReceived.strMessage, msgReceived.strMessage2);
                        break;

                    case Command.deleteChannel:
                        OnClientDeleteChannel(msgReceived.strMessage);
                        break;

                    case Command.List:
                        if (msgReceived.strMessage == "Channel") //if channel and msg is not empty (there is channels names)
                            OnClientChannelList(msgReceived.strMessage2);
                        else if (msgReceived.strMessage == "Friends")
                            OnClientFriendsList(msgReceived.strMessage2);
                        else if (msgReceived.strMessage == "ChannelsJoined")
                            OnClientChannelJoinedList(msgReceived.strMessage2);
                        else if (msgReceived.strMessage == "ChannelUsers")
                            OnClientChannelUsersList(msgReceived.strMessage2, msgReceived.strMessage3);
                        else if (msgReceived.strMessage == "IgnoredUsers")
                            OnClientIgnoredList(msgReceived.strMessage2);
                        else
                            OnClientList(msgReceived.strMessage2);
                        break;
                    case Command.manageFriend:
                        if (msgReceived.strMessage == "Add")
                            OnClientAddFriend(msgReceived.strName, msgReceived.strMessage2);
                        else if (msgReceived.strMessage == "Yes")
                            OnClientAcceptFriend(msgReceived.strName, msgReceived.strMessage2);
                        else if (msgReceived.strMessage == "Delete")
                            OnClientDeleteFriend(msgReceived.strName, msgReceived.strMessage2);
                        else
                            OnClientDenyFriend(msgReceived.strName, msgReceived.strMessage2);
                        break;
                    case Command.enterChannel:
                        if (msgReceived.strMessage2 == "enter")
                            OnClientEnterChannel(msgReceived.strMessage, msgReceived.strName, msgReceived.strMessage3);
                        else
                            OnClientEnterDenyChannel(msgReceived.strMessage3);
                        //You must first join to channel if you want to enter.
                        break;
                    case Command.leaveChannel:
                        OnClientLeaveChannel(msgReceived.strName, msgReceived.strMessage);
                        break;
                    case Command.ignoreUser:
                        if (msgReceived.strMessage == "AddIgnore")
                            OnClientIgnoreUser(msgReceived.strMessage, msgReceived.strMessage2, msgReceived.strMessage3);
                        if (msgReceived.strMessage == "DeleteIgnore")
                            OnClientDeleteIgnored(msgReceived.strMessage, msgReceived.strMessage2, msgReceived.strMessage3);
                        break;
                    /// Not Implement !!!
                    case Command.kick:
                        OnClientKickFromSerwer(msgReceived.strMessage, msgReceived.strMessage2);
                        break;
                    /// Not Implement !!!
                    case Command.ban:
                        OnClientBanFromSerwer(msgReceived.strMessage, msgReceived.strMessage2, msgReceived.strMessage3);
                        break;
                    /// Not Implement !!!
                    case Command.kickUserChannel:
                        OnClientKickFromChannel(msgReceived.strMessage, msgReceived.strMessage2, msgReceived.strMessage3);
                        break;
                    /// Not Implement !!!
                    case Command.banUserChannel:
                        break;
                }
                // Procedure listening for server messages.
                //if (msgReceived.strMessage != null && msgReceived.cmdCommand != Command.List && msgReceived.cmdCommand != Command.privMessage)
                //    OnClientMessage(msgReceived.strMessage + "\r\n");
                //else if (msgReceived.strMessage != null && msgReceived.cmdCommand == Command.privMessage && msgReceived.cmdCommand != Command.List)
                //{
                //    OnClientPrivMessage(msgReceived.strMessage + "\r\n", msgReceived.strName);
                //}

                byteData = new byte[1024];
                receiveDone.Set();

                socket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
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

        protected virtual void OnClientLostPassword(string strMessage, string strMessage2)
        {
            ClientLostPass?.Invoke(this, new ClientEventArgs() { clientChannelMsg = strMessage, clientChangePassMessage = strMessage2 });
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
        //channel todo
        protected virtual void OnClientCreateChannel(string channelName, string roomName, string userName)
        {
            ClientCreateChannel?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientChannelMsg2 = roomName, clientName = userName });
        }
        protected virtual void OnClientJoinChannel(string channelName, string channelMsg2, string channelMsg3)
        {
            ClientJoinChannel?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientChannelMsg = channelMsg2, clientChannelMsg2 = channelMsg3 });
        }
        protected virtual void OnClientDeleteChannel(string channelMsg)
        {
            ClientDeleteChannel?.Invoke(this, new ClientEventArgs() { clientChannelMsg = channelMsg });
        }
        protected virtual void OnClientExitChannel(string channelName, string channelMsg)
        {
            ClientExitChannel?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientChannelMsg = channelMsg });
        }
        protected virtual void OnClientEditChannel(string channelMsg)
        {
            ClientEditChannel?.Invoke(this, new ClientEventArgs() { clientChannelMsg = channelMsg });
        }
        protected virtual void OnClientChannelList(string channelNames)
        {
            ClientListChannel?.Invoke(this, new ClientEventArgs() { clientListChannelsMessage = channelNames });
        }
        protected virtual void OnClientChannelMessage(string channelMessage)
        {
            ClientChannelMessage?.Invoke(this, new ClientEventArgs() { clientChannelMessage = channelMessage });
        }
        protected virtual void OnClientEnterChannel(string channelName, string userName, string msg3)
        {
            ClientChannelEnter?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientName = userName, clientChannelMsg = msg3 });
        }
        protected virtual void OnClientEnterDenyChannel(string msg3)
        {
            ClientChannelEnterDeny?.Invoke(this, new ClientEventArgs() { clientChannelMsg = msg3 });
        }
        protected virtual void OnClientLeaveChannel(string userName, string channelName)
        {
            ClientChannelLeave?.Invoke(this, new ClientEventArgs() { clientName = userName, clientChannelMsg = channelName });
        }
        protected virtual void OnClientChannelJoinedList(string channelNames)
        {
            ClientListChannelJoined?.Invoke(this, new ClientEventArgs() { clientListChannelsMessage = channelNames });
        }
        protected virtual void OnClientChannelUsersList(string channelName, string usersList)
        {
            ClientListChannelUsers?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientListMessage = usersList });
        }
        //friend
        protected virtual void OnClientAddFriend(string ClientName, string ClientFriendName)
        {
            ClientAddFriend?.Invoke(this, new ClientEventArgs() { clientName = ClientName, clientFriendName = ClientFriendName });
        }
        protected virtual void OnClientAcceptFriend(string ClientName, string ClientFriendName)
        {
            ClientAcceptFriend?.Invoke(this, new ClientEventArgs() { clientName = ClientName, clientFriendName = ClientFriendName });
        }
        protected virtual void OnClientDeleteFriend(string ClientName, string ClientFriendName)
        {
            ClientDeleteFriend?.Invoke(this, new ClientEventArgs() { clientName = ClientName, clientFriendName = ClientFriendName });
        }
        protected virtual void OnClientDenyFriend(string ClientName, string ClientFriendName)
        {
            ClientDenyFriend?.Invoke(this, new ClientEventArgs() { clientName = ClientName, clientFriendName = ClientFriendName });
        }
        protected virtual void OnClientFriendsList(string friendNames)
        {
            ClientListFriends?.Invoke(this, new ClientEventArgs() { clientListFriendsMessage = friendNames });
        }
        //ignore
        protected virtual void OnClientIgnoredList(string usersList)
        {
            ClientListIgnored?.Invoke(this, new ClientEventArgs() { clientListMessage = usersList });
        }
        protected virtual void OnClientIgnoreUser(string ignoreOption, string ignoreMessage, string ignoredName)
        {
            ClientIgnoreUser?.Invoke(this, new ClientEventArgs() { clientIgnoreOption = ignoreOption, clientIgnoreMessage = ignoreMessage, clientIgnoreName = ignoredName });
        }
        protected virtual void OnClientDeleteIgnored(string ignoreOption, string ignoreMessage, string ignoredName)
        {
            ClientDeleteIgnoredUser?.Invoke(this, new ClientEventArgs() { clientIgnoreOption = ignoreOption, clientIgnoreMessage = ignoreMessage, clientIgnoreName = ignoredName });
        }
        //kick/ban
        protected virtual void OnClientKickFromChannel(string userName, string kickReason, string channelName)
        {
            ClientKickFromChannel?.Invoke(this, new ClientEventArgs() { clientName = userName, clientKickReason = kickReason, clientChannelName = channelName });
        }
        protected virtual void OnClientKickFromSerwer(string userName, string kickReason)
        {
            ClientKickFromSerwer?.Invoke(this, new ClientEventArgs() { clientName = userName, clientKickReason = kickReason });
        }
        protected virtual void OnClientBanFromSerwer(string userName, string time, string kickReason)
        {
            ClientBanFromSerwer?.Invoke(this, new ClientEventArgs() { clientName = userName, clientBanTime = time, clientBanReason = kickReason });
        }
    }
}
