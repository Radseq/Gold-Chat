using CommandClient;
using Gold_Client.Model;
using System;
using System.Net.Sockets;
using System.Threading;

namespace Gold_Client.ViewModel
{
    public class ClientReceivedFromServer : IClient
    {
        //For login
        public event EventHandler<ClientEventArgs> ClientRegistration;
        public event EventHandler<ClientEventArgs> ClientReSendEmail;
        public event EventHandler<ClientEventArgs> ReceiveLogExcep;
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

        public Client User
        {
            get; set;
        }

        private byte[] byteData = new byte[1024];

        //lets use config
        public Configuration config = new Configuration();

        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        public ClientReceivedFromServer()
        {
            config = config.loadConfig();
            User = App.Client;

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


        public void BeginReceive()
        {
            byteData = new byte[1024];
            User.cSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
            receiveDone.WaitOne();
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                //var so = (StateObject)ar.AsyncState;

                if (!User.cSocket.Connected) return;

                User.cSocket.EndReceive(ar);

                Data msgReceived = new Data(byteData);
                //Accordingly process the message received
                switch (msgReceived.cmdCommand)
                {
                    case Command.Login:
                        if (msgReceived.strName == User.strName && msgReceived.strMessage == "You are succesfully Log in") // && msgReceived.loginName != userName
                        {
                            OnClientSuccesLogin(true, User.cSocket);
                            User.permission = int.Parse(msgReceived.strMessage2);
                        }
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

                    case Command.ReSendActiveCode:
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
                        OnClientPrivMessage(msgReceived.strMessage + "\r\n", msgReceived.strName);
                        break;

                    case Command.createChannel:
                        OnClientCreateChannel(msgReceived.strMessage, msgReceived.strMessage2);
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

                User.cSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
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
        protected virtual void OnClientCreateChannel(string channelMsg, string roomName)
        {
            ClientCreateChannel?.Invoke(this, new ClientEventArgs() { clientChannelMsg = channelMsg, clientChannelMsg2 = roomName });
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