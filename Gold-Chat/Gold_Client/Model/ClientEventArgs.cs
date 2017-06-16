using System;
using System.Net.Sockets;

namespace Gold_Client.Model
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
        public string clientSendActivCodeFromEmail { get; set; }
        public string receiveLogExpceMessage { get; set; }

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
}
