using CommandClient;
using Gold_Client.Model;
using Gold_Client.ProgramableWindow;
using Gold_Client.View;
using Gold_Client.ViewModel.Others;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Gold_Client.ViewModel.TabWindows
{
    class ChannelPresenter : ObservableObject
    {
        ProcessReceivedByte getMessageFromServer = ProcessReceivedByte.Instance;
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;

        public string channelName = "";
        public string WelcomeChannelMsg = "";

        string selectedUser;

        public ChannelPresenter()
        {
            getMessageFromServer.ClientChannelMessage += OnClientChannelMessage;
            getMessageFromServer.ClientLogout += ClientLogout;
            getMessageFromServer.ClientChannelEnter += OnClientChannelEnter;
            getMessageFromServer.ClientListChannelUsers += OnClientListChannelUsers;
            getMessageFromServer.ClientChannelLeave += OnClientChannelLeave;
            getMessageFromServer.ClientKickFromChannel += OnClientKickFromChannel;
            getMessageFromServer.ClientBanFromChannel += OnClientBanFromChannel;

            showMessage("<<< Welcome Message: " + WelcomeChannelMsg + " >>>>");
        }

        private void OnClientBanFromChannel(object sender, ClientEventArgs e)
        {
            if (e.clientName != App.Client.strName)
            {
                showMessage("User " + e.clientName + e.clientBanReason);
            }
        }

        private void OnClientKickFromChannel(object sender, ClientEventArgs e)
        {
            if (e.clientName != App.Client.strName)
            {
                showMessage("User " + e.clientName + e.clientKickReason);
            }
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

        private void AddToChannelUserList(string userName)
        {
            if (!channelUsers.Contains(userName))
                channelUsers.Add(userName);
        }

        private void RemoveFromChannelUserList(string userName)
        {
            if (!channelUsers.Contains(userName))
                channelUsers.Remove(userName);
        }

        private void OnClientListChannelUsers(object sender, ClientEventArgs e)
        {
            string[] splitNicks = e.clientListMessage.Split('*').Where(value => value != "").ToArray(); ;
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
                    showMessage("<<< " + e.clientName + " has leave this channel>>>");
                    RemoveFromChannelUserList(user);
                    break;
                }
            }
        }

        private void OnClientChannelEnter(object sender, ClientEventArgs e)
        {
            showMessage("<<< " + e.clientName + " log into channel");
            channelUsers.Add(e.clientName);
        }

        private void ClientLogout(object sender, ClientEventArgs e)
        {
            foreach (string user in channelUsers)
            {
                if (user == e.clientLogoutMessage)
                {
                    showMessage("<<< " + e.clientLogoutMessage + " has logout>>>");
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
            ChannelMsgReceived += message + "\r\n";
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

        public string SelectedUser
        {
            get { return selectedUser; }

            set
            {
                selectedUser = value;
                RaisePropertyChangedEvent(nameof(SelectedUser));
            }
        }

        public string KickUserHeaderCommand
        {
            get { return "Kick " + selectedUser + " from channel?"; }
        }

        public ICommand KickUserCommand => new DelegateCommand(() =>
        {
            if (ChannelUsers.Contains(selectedUser))
            {
                GenerateTexBoxWindow createTextWindow = new GenerateTexBoxWindow();
                ChannelKickUserReason.ChannelName = channelName;
                ChannelKickUserReason.UserName = selectedUser;
                createTextWindow.OnClickOrEnter += ChannelKickUserReason.OnClickOrEnter;
                createTextWindow.createWindow("Kick: " + selectedUser + " reason", "Give reason of kicking: " + selectedUser);
            }
        });

        public string DeleteUserHeaderCommand
        {
            get { return "Delete " + selectedUser + " from channel?"; }
        }

        public ICommand DeleteUserCommand => new DelegateCommand(() =>
        { // todo msg box yes|no to delete user, user will need to join again using password
            //if (ChannelUsers.Contains(selectedUser))
        });

        public string BanUserHeaderCommand
        {
            get { return "Ban " + selectedUser + " from channel?"; }
        }

        public ICommand BanUserCommand => new DelegateCommand(() =>
        {
            if (ChannelUsers.Contains(selectedUser))
            {
                BanUserWindow banWindow = new BanUserWindow(selectedUser, channelName);
                banWindow.Show();
            }
        });
    }
}
