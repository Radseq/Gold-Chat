using CommandClient;
using Gold_Client.Model;
using System;
using System.Net.Sockets;

namespace Gold_Client.ViewModel.Others
{
    public class ProcessReceivedByte
    {
        //ClientReceivedFromServer clientReceive = ClientReceivedFromServer.Instance;

        // Singleton
        static ProcessReceivedByte instance = null;
        static readonly object padlock = new object();

        // Singleton
        public static ProcessReceivedByte Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new ProcessReceivedByte();

                    return instance;
                }
            }
        }

        private ProcessReceivedByte()
        {
            //clientReceive.OnDataReceived += ProccesData;
            ReceivePackageFromServer.OnDataReceived += ProccesData;
        }

        //For login
        public event EventHandler<ClientEventArgs> ClientRegistration;
        public event EventHandler<ClientEventArgs> ClientSendActivCodeFromEmail;
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
        public event EventHandler<ClientEventArgs> ClientExitChannel;
        public event EventHandler<ClientEventArgs> ClientEditChannel;
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

        public event EventHandler<ClientEventArgs> ClientBanFromChannel;
        public event EventHandler<ClientEventArgs> ClientKickFromServer;
        public event EventHandler<ClientEventArgs> ClientBanFromServer;
        //public event EventHandler<ClientEventArgs> ClientListIgnored;
        public event EventHandler<ClientEventArgs> ClientReceiveFile;
        public event EventHandler<ClientEventArgs> ClientReceiveFileInfo;

        private void ProccesData(object sender, EventArgs e)
        {
            Data msgReceived = new Data(App.Client.Buffer);
            //Accordingly process the message received
            switch (msgReceived.cmdCommand)
            {
                case Command.Login:
                    if (msgReceived.strName == App.Client.strName && msgReceived.strMessage == "You are succesfully Log in") // && msgReceived.loginName != userName
                    {
                        OnClientSuccesLogin(true, App.Client.cSocket);
                        App.Client.permission = int.Parse(msgReceived.strMessage2);
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
                    OnClientLostPassword(msgReceived.strMessage);
                    break;

                case Command.SendActivationCode:
                    OnClientSendActivCodeFromEmail(msgReceived.strMessage);
                    break;

                case Command.Logout:
                    OnClientLogout(msgReceived.strName);
                    break;

                case Command.Message:
                    if (msgReceived.strMessage2 == null) //strMessage2 -> ChannelName
                        OnClientMessage(msgReceived.strMessage);
                    else
                        OnClientChannelMessage(msgReceived.strMessage); //strMessage -> ChannelMessage
                    break;

                case Command.privMessage:
                    OnClientPrivMessage(msgReceived.strMessage, msgReceived.strMessage2);
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

                case Command.editChannel:
                    // ToDo All
                    break;

                case Command.deleteChannel:
                    OnClientDeleteChannel(msgReceived.strMessage, msgReceived.strMessage2);
                    break;

                case Command.List:
                    if (msgReceived.strMessage == "Channel")
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

                case Command.kick:
                    OnClientKickFromSerwer(msgReceived.strMessage, msgReceived.strMessage2);
                    break;

                case Command.ban:
                    OnClientBanFromServer(msgReceived.strMessage, msgReceived.strMessage2);
                    break;

                case Command.kickUserChannel:
                    OnClientKickFromChannel(msgReceived.strMessage, msgReceived.strMessage2, msgReceived.strMessage3);
                    break;

                case Command.banUserChannel:
                    OnClientBanFromChannel(msgReceived.strMessage, msgReceived.strMessage2, msgReceived.strMessage4);
                    break;

                case Command.sendFile:
                    if (msgReceived.strFileMsg != null)
                        OnClientReceiveFile(msgReceived.strMessage, msgReceived.strMessage2, msgReceived.strFileMsg);
                    else
                        OnClientReceiveFileInfo(msgReceived.strMessage, msgReceived.strMessage2, msgReceived.strMessage3, msgReceived.strName);
                    break;
            }
        }

        protected virtual void OnClientLostPassword(string strMessage)
        {
            ClientLostPass?.Invoke(this, new ClientEventArgs() { clientMessage = strMessage });
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

        protected virtual void OnClientSendActivCodeFromEmail(string activCodeFromEmailMessage)
        {
            ClientSendActivCodeFromEmail?.Invoke(this, new ClientEventArgs() { clientSendActivCodeFromEmail = activCodeFromEmailMessage });
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

        protected virtual void OnClientCreateChannel(string channelMsg, string roomName, string creatorName)
        {
            ClientCreateChannel?.Invoke(this, new ClientEventArgs() { clientChannelMsg = channelMsg, clientChannelMsg2 = roomName, clientName = creatorName });
        }
        protected virtual void OnClientJoinChannel(string channelName, string channelMsg2, string channelMsg3)
        {
            ClientJoinChannel?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientChannelMsg = channelMsg2, clientChannelMsg2 = channelMsg3 });
        }
        protected virtual void OnClientDeleteChannel(string channelMsg, string channelMsg2)
        {
            ClientDeleteChannel?.Invoke(this, new ClientEventArgs() { clientChannelMsg = channelMsg, clientChannelMsg2 = channelMsg2 });
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
        protected virtual void OnClientBanFromChannel(string userName, string banReason, string channelName)
        {
            ClientBanFromChannel?.Invoke(this, new ClientEventArgs() { clientName = userName, clientBanReason = banReason, clientChannelName = channelName });
        }
        protected virtual void OnClientKickFromSerwer(string userName, string kickReason)
        {
            ClientKickFromServer?.Invoke(this, new ClientEventArgs() { clientName = userName, clientKickReason = kickReason });
        }
        protected virtual void OnClientBanFromServer(string userName, string banReason)
        {
            ClientBanFromServer?.Invoke(this, new ClientEventArgs() { clientName = userName, clientBanReason = banReason });
        }
        // File or Respond From server need to change names to messages because names wrong tells whats happend
        protected virtual void OnClientReceiveFile(string friendName, string fileName, Byte[] fileByteReceive)
        {
            ClientReceiveFile?.Invoke(this, new ClientEventArgs() { clientFriendName = friendName, FileName = fileName, FileByte = fileByteReceive });
        }

        protected virtual void OnClientReceiveFileInfo(string friendName, string fileLen, string fileName, string ClientName)
        {
            ClientReceiveFileInfo?.Invoke(this, new ClientEventArgs() { clientFriendName = friendName, FileLen = fileLen, FileName = fileName, clientName = ClientName });
        }
    }
}
