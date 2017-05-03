using CommandClient;
using Gold_Client.Model;
using Gold_Client.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Gold_Client.ViewModel
{
    public class MainWindowPresenter : ObservableObject, IClient
    {
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;
        ClientReceivedFromServer clientReceiveFromServer = ClientReceivedFromServer.Instance;

        private readonly ObservableCollection<string> usersConnected = new ObservableCollection<string>();
        private readonly ObservableCollection<string> friendlyUsersConnected = new ObservableCollection<string>();
        private readonly ObservableCollection<string> ignoredUsers = new ObservableCollection<string>();
        private readonly ObservableCollection<string> lobbies = new ObservableCollection<string>();
        private readonly ObservableCollection<string> joinedChannelsList = new ObservableCollection<string>();
        public IEnumerable<string> UsersConnected => usersConnected;
        public IEnumerable<string> FriendlyUsersConnected => friendlyUsersConnected;
        public IEnumerable<string> IgnoredUsers => ignoredUsers;
        public IEnumerable<string> Lobbies => lobbies;
        public IEnumerable<string> JoinedChannelsList => joinedChannelsList;

        private string incomeMsg;
        private string outcomeMsg;
        private string selectedUser;
        private string selectedFriendlyUser;
        private string selectedIgnoredUser;
        private string selectedLobbie;
        private string selectedJoinedLobbie;

        public MainWindowPresenter()
        {
            clientReceiveFromServer.ClientLogin += OnClientLogin;
            clientReceiveFromServer.ClientLogout += OnClientLogout;
            clientReceiveFromServer.ClientList += OnClientList;
            clientReceiveFromServer.ClientMessage += OnClientMessage;
            clientReceiveFromServer.ClientPrivMessage += OnClientPrivMessage;
            clientReceiveFromServer.ClientChangePass += (s, e) => MessageBox.Show(e.clientChangePassMessage, "Gold Chat: " + Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            clientReceiveFromServer.ReceiveLogExcep += (s, e) => MessageBox.Show(e.receiveLogExpceMessage, "Gold Chat: " + Client.strName, MessageBoxButton.OK, MessageBoxImage.Error);
            //clientReceiveFromServer.SendException += (s, e) => MessageBox.Show(e.sendExcepMessage, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Error);
            //channels
            clientReceiveFromServer.ClientCreateChannel += OnClientCreateChannel;
            clientReceiveFromServer.ClientDeleteChannel += (s, e) => MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            clientReceiveFromServer.ClientEditChannel += (s, e) => MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            clientReceiveFromServer.ClientJoinChannel += OnClientJoinChannel;
            clientReceiveFromServer.ClientExitChannel += OnClientExitChannel;
            clientReceiveFromServer.ClientListChannel += OnClientListChannel;
            clientReceiveFromServer.ClientChannelEnter += OnClientChannelEnter;
            clientReceiveFromServer.ClientChannelEnterDeny += (s, e) => MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            clientReceiveFromServer.ClientListChannelJoined += OnClientListChannelJoined;
            //friends
            clientReceiveFromServer.ClientAddFriend += OnClientAddFriend;
            clientReceiveFromServer.ClientAcceptFriend += OnClientAcceptFriend;
            clientReceiveFromServer.ClientListFriends += OnClientListFriends;
            clientReceiveFromServer.ClientDeleteFriend += OnClientDeleteFriend;
            clientReceiveFromServer.ClientDenyFriend += (s, e) => MessageBox.Show("User: " + e.clientFriendName + " doesnt accept your ask to be your friend", "Gold Chat: " + Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            clientReceiveFromServer.ClientIgnoreUser += OnClientIgnoreUser;
            clientReceiveFromServer.ClientDeleteIgnoredUser += OnClientDeleteIgnoredUser;
            clientReceiveFromServer.ClientListIgnored += OnClientListIgnored;
            //ban/kick
            clientReceiveFromServer.ClientKickFromSerwer += OnClientKickFromSerwer;
            clientReceiveFromServer.ClientBanFromSerwer += OnClientBanFromSerwer;
        }

        public Client Client
        {
            get; set;
        }

        public string IncomeMessageTB
        {
            get { return incomeMsg; }
            set
            {
                incomeMsg = value;
                RaisePropertyChangedEvent(nameof(IncomeMessageTB));
            }
        }

        public string OutcomeMessageTB
        {
            get { return outcomeMsg; }
            set
            {
                outcomeMsg = value;
                RaisePropertyChangedEvent(nameof(OutcomeMessageTB));
            }
        }
        #region Selected ListBox Commands
        public string SelectedUser
        {
            get { return selectedUser; }

            set
            {
                selectedUser = value;
                RaisePropertyChangedEvent(nameof(SelectedUser));
            }
        }

        public string SelectedFriendlyUser
        {
            get { return selectedFriendlyUser; }

            set
            {
                selectedFriendlyUser = value;
                RaisePropertyChangedEvent(nameof(SelectedFriendlyUser));
            }
        }

        public string SelectedIgnoredUser
        {
            get { return selectedIgnoredUser; }

            set
            {
                selectedIgnoredUser = value;
                RaisePropertyChangedEvent(nameof(SelectedIgnoredUser));
            }
        }

        public string SelectedLobbie
        {
            get { return selectedLobbie; }

            set
            {
                selectedLobbie = value;
                RaisePropertyChangedEvent(nameof(SelectedLobbie));
            }
        }

        public string SelectedJoinedLobbie
        {
            get { return selectedJoinedLobbie; }

            set
            {
                selectedJoinedLobbie = value;
                RaisePropertyChangedEvent(nameof(SelectedJoinedLobbie));
            }
        }
        #endregion

        public ICommand MessageCommand => new DelegateCommand(() =>
        {
            if (string.IsNullOrWhiteSpace(OutcomeMessageTB)) return;
            clientSendToServer.SendToServer(Command.Message, OutcomeMessageTB);
            OutcomeMessageTB = string.Empty;
        });

        #region User Buttons Command
        public ICommand ControlPanelTabCommand => new DelegateCommand(() =>
        {

        });

        public ICommand CreateRoomTabCommand => new DelegateCommand(() =>
        {

        });

        public ICommand SugestionTabCommand => new DelegateCommand(() =>
        {

        });

        public ICommand AbouseTabCommand => new DelegateCommand(() =>
        {

        });

        public ICommand ArchiveTabCommand => new DelegateCommand(() =>
        {
        });

        public ICommand ContactTabCommand => new DelegateCommand(() =>
        {

        });

        public ICommand AdminTabCommand => new DelegateCommand(() =>
        {

        });

        public ICommand InformationTabCommand => new DelegateCommand(() =>
        {

        });

        public ICommand LogoutCommand => new DelegateCommand(() =>
        {

        });

        #region ListBox Menu Items Buttons
        public ICommand AddFriendHandleCommand => new DelegateCommand(() =>
        {

        });

        public ICommand PrivateMsgToUserCommand => new DelegateCommand(() =>
        {

        });

        public ICommand IgnoreUserCommand => new DelegateCommand(() =>
        {

        });

        public ICommand PrivateMsgToFriendCommand => new DelegateCommand(() =>
        {

        });

        public ICommand DeleteFriendCommand => new DelegateCommand(() =>
        {

        });

        public ICommand DeleteIgnoredUserCommand => new DelegateCommand(() =>
        {

        });

        public ICommand JoinToLobbieCommand => new DelegateCommand(() =>
        {

        });

        public ICommand EnterToJoinedChannelCommand => new DelegateCommand(() =>
        {

        });

        public ICommand LeaveChannelCommand => new DelegateCommand(() =>
        {

        });

        public ICommand ExitChannelCommand => new DelegateCommand(() =>
        {

        });

        public ICommand DeleteChannelCommand => new DelegateCommand(() =>
        {

        });
        #endregion

        #endregion

        private void OnClientLogin(object sender, ClientEventArgs e)
        {
            if (!usersConnected.Contains(e.clientLoginName))
            {
                usersConnected.Add(e.clientLoginName);
                IncomeMessageTB += e.clientLoginMessage + "\r\n";
            }
        }

        private void OnClientLogout(object sender, ClientEventArgs e)
        {
            usersConnected.Remove(e.clientLogoutMessage);
            IncomeMessageTB += "<<<" + e.clientLogoutMessage + " has left the room>>>" + "\r\n";
        }

        private void OnClientList(object sender, ClientEventArgs e)
        {
            string[] splitNicks = e.clientListMessage.Split('*');
            foreach (string nick in splitNicks)
            {
                if (!usersConnected.Contains(nick))
                    usersConnected.Add(nick);
            }
            //clientList.AddRange(e.clientListMessage.Split('*'));
            usersConnected.RemoveAt(usersConnected.Count - 1);

        }

        private void OnClientMessage(object sender, ClientEventArgs e)
        {
            IncomeMessageTB += e.clientMessage;
        }

        private void OnClientPrivMessage(object sender, ClientEventArgs e)
        {
            PrivateMessageWindow privateMessage = new PrivateMessageWindow();
            privateMessage.Show();

            //if (pm == null)
            //{
            //    pm = new private_message(e.clientFriendName);
            //    pm.Show();
            //    pm.showPrivMessageTb.Text += e.clientPrivMessage;
            //}
            //pm.showPrivMessageTb.Text += e.clientPrivMessage;

        }

        private void OnClientCreateChannel(object sender, ClientEventArgs e)
        {
            MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            if (e.clientChannelMsg2 == "CreatedChannel")
            {
                lobbies.Add(e.clientChannelName);
            }
        }

        private void OnClientJoinChannel(object sender, ClientEventArgs e)
        {
            if (e.clientChannelMsg == "Send Password" || e.clientChannelMsg == "Wrong Password")
            {
                //i need programmatically create window
                MessageBox.Show("Send Password to channel " + e.clientChannelName, "Gold Chat: " + Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);

                //serverAsk sa = new serverAsk(clientManager, e.clientChannelName, e.clientChannelMsg);
                //sa.Show();
            }
            else if (e.clientChannelMsg2 == "ChannelJoined" || e.clientChannelMsg2 == "CreatedChannel")
            {
                joinedChannelsList.Add(e.clientChannelName);
                MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
                MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnClientExitChannel(object sender, ClientEventArgs e)
        {
            if (e.clientChannelMsg == "You are exit from the channel")
            {
                MessageBox.Show(e.clientChannelMsg + " " + e.clientChannelName + ".", "Gold Chat: " + Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);

                joinedChannelsList.Remove(e.clientChannelName);

            }
            else
                MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnClientListChannel(object sender, ClientEventArgs e)
        {
            string[] splitChannels = e.clientListChannelsMessage.Split('*');
            foreach (string channel in splitChannels)
            {
                if (!lobbies.Contains(channel))
                    lobbies.Add(channel);
            }

            //clientChannelsList.AddRange(e.clientListChannelsMessage.Split('*'));
            //lobbies.RemoveAt(lobbies.Count - 1);
        }

        private void OnClientChannelEnter(object sender, ClientEventArgs e)
        {
            if (e.clientName == Client.strName)
            {
                //string channelName = e.clientChannelName;
                //tab_windows.channelWindow channelPanel = new tab_windows.channelWindow(channelName);

                //var header = new TextBlock { Text = channelName };
                //// Create the tab
                //var tab = new CloseableTabItem();
                //tab.SetHeader(header, channelName, true);
                //tab.Content = channelPanel;

                //// Add to TabControl
                //tc.Items.Add(tab);

                ////user enter to channel and want a list of all users in
                //clientSendToServer.SendToServer(Command.List, "ChannelUsers", channelName);

                ////lets print motd 
                //channelPanel.channelMessages.Text += "<<< Welcome Message: " + e.clientChannelMsg + " >>>>" + "\r\n";
            }
        }

        private void OnClientListChannelJoined(object sender, ClientEventArgs e)
        {
            string[] splitChannels = e.clientListChannelsMessage.Split('*');
            foreach (string channel in splitChannels)
            {
                if (!joinedChannelsList.Contains(channel))
                    joinedChannelsList.Add(channel);
            }
            //joinedChannelsList.AddRange(e.clientListChannelsMessage.Split('*'));
            //joinedChannelsList.RemoveAt(clientChannelsJoinedList.Count - 1);
            //lbJoinedChann.ItemsSource = clientChannelsJoinedList;
        }

        private void OnClientAddFriend(object sender, ClientEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("User: " + e.clientFriendName + " want to be your friend Accept?", "Gold Chat: " + Client.strName, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                clientSendToServer.SendToServer(Command.manageFriend, "No", e.clientFriendName);
            else
                clientSendToServer.SendToServer(Command.manageFriend, "Yes", e.clientFriendName);
        }

        private void OnClientAcceptFriend(object sender, ClientEventArgs e)
        {
            friendlyUsersConnected.Add(e.clientFriendName == Client.strName ? e.clientName : e.clientFriendName);

            MessageBox.Show("You are now friend with: " + (e.clientFriendName == Client.strName ? e.clientName : e.clientFriendName), "Gold Chat: " + Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnClientListFriends(object sender, ClientEventArgs e)
        {
            string[] listFriends = e.clientListFriendsMessage.Split('*');
            foreach (string friendName in listFriends)
            {
                if (!friendlyUsersConnected.Contains(friendName))
                    friendlyUsersConnected.Add(friendName);
            }
        }

        //After we got msg from server that we/friend delete as/friend we need got list of friends
        private void OnClientDeleteFriend(object sender, ClientEventArgs e)
        {
            friendlyUsersConnected.Remove(e.clientFriendName == Client.strName ? e.clientName : e.clientFriendName);
        }

        private void OnClientIgnoreUser(object sender, ClientEventArgs e)
        {
            MessageBox.Show(e.clientIgnoreMessage, "Gold Chat: " + Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            if (e.clientIgnoreOption == "AddIgnore")
                ignoredUsers.Add(e.clientIgnoreName);
        }

        private void OnClientDeleteIgnoredUser(object sender, ClientEventArgs e)
        {
            MessageBox.Show(e.clientIgnoreMessage, "Gold Chat: " + Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            if (e.clientIgnoreOption == "DeleteIgnore")
            {
                ignoredUsers.Remove(e.clientIgnoreName);
                clientSendToServer.SendToServer(Command.List);
            }
        }

        private void OnClientListIgnored(object sender, ClientEventArgs e)
        {
            string[] ignoredUsersFromServer = e.clientListFriendsMessage.Split('*');
            foreach (string name in ignoredUsersFromServer)
            {
                if (!ignoredUsers.Contains(name))
                    ignoredUsers.Add(name);
            }
        }

        private void OnClientKickFromSerwer(object sender, ClientEventArgs e)
        {
            if (e.clientName == Client.strName)
            {
                MessageBox.Show("You are kicked reason: " + e.clientKickReason, "Gold Chat: " + Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);
                //Close();
            }
            else
            {
                usersConnected.Remove(e.clientName);
                incomeMsg += e.clientName + " has kicked reason: " + e.clientKickReason + "\r\n";
            }
        }

        private void OnClientBanFromSerwer(object sender, ClientEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
