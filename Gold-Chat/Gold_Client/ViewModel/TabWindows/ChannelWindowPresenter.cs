using CommandClient;
using Gold_Client.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Gold_Client.ViewModel.TabWindows
{
    class ChannelPresenter : ObservableObject
    {
        ClientReceivedFromServer received = ClientReceivedFromServer.Instance;
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;

        string channelName;

        public ChannelPresenter()
        {
            received.ClientChannelMessage += OnClientChannelMessage;
            received.ClientLogout += ClientLogout;
            received.ClientChannelEnter += OnClientChannelEnter;
            received.ClientListChannelUsers += OnClientListChannelUsers;
            received.ClientChannelLeave += OnClientChannelLeave;
        }

        private readonly ObservableCollection<string> channelUsers = new ObservableCollection<string>();
        public IEnumerable<string> ChannelUsers => channelUsers;

        private string channelMsgReceived;
        public string ChannelMsgReceived
        {
            get { return channelMsgReceived; }
            set
            {
                channelMsgReceived = value;
                RaisePropertyChangedEvent(nameof(ChannelMsgReceived));
            }
        }

        private string channelMsgToSend;
        public string ChannelMsgToSend
        {
            get { return channelMsgToSend; }
            set
            {
                channelMsgToSend = value;
                RaisePropertyChangedEvent(nameof(ChannelMsgToSend));
            }
        }

        private void AddToChannelUserList(string item)
        {
            if (!channelUsers.Contains(item))
                channelUsers.Add(item);
        }

        private void RemoveFromChannelUserList(string item)
        {
            if (!channelUsers.Contains(item))
                channelUsers.Remove(item);
        }

        private void OnClientListChannelUsers(object sender, ClientEventArgs e)
        {
            //List<string> channelUsersList = new List<string>();

            string[] splitNicks = e.clientListMessage.Split('*');
            //channelUsersList.AddRange(e.clientListMessage.Split('*'));
            //channelUsersList.RemoveAt(channelUsersList.Count - 1);
            foreach (string name in splitNicks)
            {
                AddToChannelUserList(name);
            }
        }

        private void OnClientChannelLeave(object sender, ClientEventArgs e)
        {
            foreach (string user in channelUsers)
            {
                if (user == e.clientName)
                {
                    showMessage("<<< " + e.clientName + " has leave this channel>>>" + "\r\n");
                    RemoveFromChannelUserList(user);
                    break;
                }
            }
        }

        private void OnClientChannelEnter(object sender, ClientEventArgs e)
        {
            showMessage("<<< " + e.clientName + " log into channel" + "\r\n");
            channelUsers.Add(e.clientName);
        }

        private void ClientLogout(object sender, ClientEventArgs e)
        {
            foreach (string user in channelUsers)
            {
                if (user == e.clientLogoutMessage)
                {
                    showMessage("<<< " + e.clientLogoutMessage + " has logout>>>" + "\r\n");
                    RemoveFromChannelUserList(user);
                    break;
                }
            }
        }

        private void OnClientChannelMessage(object sender, ClientEventArgs e)
        {
            showMessage(e.clientChannelMessage);
        }

        private void showMessage(string message)
        {
            channelMsgReceived += message;
        }

        private void SendMessage()
        {
            clientSendToServer.SendToServer(Command.Message, ChannelMsgToSend, channelName);
        }

        public ICommand SendChannelMsgCommand => new DelegateCommand(() =>
        {
            if (string.IsNullOrWhiteSpace(ChannelMsgToSend)) return;
            SendMessage();
            ChannelMsgToSend = string.Empty;
        });
    }
}
