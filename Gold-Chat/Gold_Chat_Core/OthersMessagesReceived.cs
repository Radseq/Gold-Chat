using Gold_Chat_Core.SocketWorking;
using System;
using System.Windows;

namespace Gold_Chat_Core
{
    class OthersMessagesReceived
    {
        ProcessReceivedByte proccesReceiverInformation = ProcessReceivedByte.Instance;

        public OthersMessagesReceived()
        {
            proccesReceiverInformation.ClientEditChannel += (s, e) => MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            proccesReceiverInformation.ClientChannelEnterDeny += (s, e) => MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            proccesReceiverInformation.ClientDenyFriend += (s, e) => MessageBox.Show("User: " + e.clientFriendName + " doesnt accept your ask to be your friend", "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            proccesReceiverInformation.ClientReceiveFileInfo += OnClientReceiveFileInfo;
            proccesReceiverInformation.ClientAddFriend += OnClientAddFriend;
        }

        private void OnClientReceiveFileInfo(object sender, ClientEventArgs e)
        {
            if (e.clientName == App.Client.strName && e.FileLen != "AcceptReceive")
            {
                ReceiveFileWindow getFileInfo = new ReceiveFileWindow(e.clientFriendName, e.FileName, Convert.ToInt64(e.FileLen));
                getFileInfo.Show();
            }
        }

        private void OnClientPrivMessage(object sender, ClientEventArgs e)
        {
            if (privateMessage == null)
            {
                privateMessage = new PrivateMessageWindow(e.clientFriendName, e.clientPrivMessage);
                privateMessage.ShowDialog();
            }
        }

        private void OnClientAddFriend(object sender, ClientEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("User: " + e.clientFriendName + " want to be your friend Accept?", "Gold Chat: " + User.strName, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                clientSendToServer.SendToServer(Command.manageFriend, "No", e.clientFriendName);
            else
                clientSendToServer.SendToServer(Command.manageFriend, "Yes", e.clientFriendName);
        }
    }
}
