using CommandClient;
using Gold_Client.Model;
using Gold_Client.View;
using Gold_Client.View.TabWindows;
using Gold_Client.ViewModel.Others;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gold_Client.ViewModel
{
    public class MainContentPresenter : ObservableObject, IClient
    {
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;
        ClientReceivedFromServer clientReceiveFromServer = ClientReceivedFromServer.Instance;
        ProcessReceivedByte proccesReceiverInformation = ProcessReceivedByte.Instance;

        PrivateMessageWindow privateMessage;

        /// HACK -> send list of tab items to CloseableTabItem and there remove TabItem
        private/* readonly*/ ObservableCollection<object> tabControlItems = new ObservableCollection<object>();
        //public IEnumerable<object> TabControlItems => tabControlItems;
        public ObservableCollection<object> TabControlItems { get { return tabControlItems; } }

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

        private string selectedUser;
        private string selectedFriendlyUser;
        private string selectedIgnoredUser;
        private string selectedLobbie;
        private string selectedJoinedLobbie;

        public MainContentPresenter()
        {
            proccesReceiverInformation.ClientLogin += OnClientLogin;
            proccesReceiverInformation.ClientLogout += OnClientLogout;
            proccesReceiverInformation.ClientList += OnClientList;
            proccesReceiverInformation.ClientPrivMessage += OnClientPrivMessage;
            proccesReceiverInformation.ClientChangePass += (s, e) => MessageBox.Show(e.clientChangePassMessage, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);

            clientReceiveFromServer.ReceiveLogExcep += (s, e) => MessageBox.Show(e.receiveLogExpceMessage, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Error);
            //proccesReceiverInformation.SendException += (s, e) => MessageBox.Show(e.sendExcepMessage, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Error);
            //channels
            proccesReceiverInformation.ClientCreateChannel += OnClientCreateChannel;
            proccesReceiverInformation.ClientDeleteChannel += (s, e) => MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            proccesReceiverInformation.ClientEditChannel += (s, e) => MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            //proccesReceiverInformation.ClientJoinChannel += OnClientJoinChannel; // all in new class
            proccesReceiverInformation.ClientExitChannel += OnClientExitChannel;
            proccesReceiverInformation.ClientListChannel += OnClientListChannel;
            proccesReceiverInformation.ClientChannelEnter += OnClientChannelEnter;
            proccesReceiverInformation.ClientChannelEnterDeny += (s, e) => MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            proccesReceiverInformation.ClientListChannelJoined += OnClientListChannelJoined;
            //friends
            proccesReceiverInformation.ClientAddFriend += OnClientAddFriend;
            proccesReceiverInformation.ClientAcceptFriend += OnClientAcceptFriend;
            proccesReceiverInformation.ClientListFriends += OnClientListFriends;
            proccesReceiverInformation.ClientDeleteFriend += OnClientDeleteFriend;
            proccesReceiverInformation.ClientDenyFriend += (s, e) => MessageBox.Show("User: " + e.clientFriendName + " doesnt accept your ask to be your friend", "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            proccesReceiverInformation.ClientIgnoreUser += OnClientIgnoreUser;
            proccesReceiverInformation.ClientDeleteIgnoredUser += OnClientDeleteIgnoredUser;
            proccesReceiverInformation.ClientListIgnored += OnClientListIgnored;
            //ban/kick
            proccesReceiverInformation.ClientKickFromSerwer += OnClientKickFromSerwer;
            proccesReceiverInformation.ClientBanFromSerwer += OnClientBanFromSerwer;
            InformServerToSendUserLists informServerToSendUserLists = new InformServerToSendUserLists();

            addTab(new GlobalMessageContent(), "Main");
        }

        public Client User
        {
            get; set;
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

        #region User Buttons Command

        private void addTab<T>(T tabInstance, string tabName, bool channel = false)
        {
            var header = new TextBlock { Text = tabName };

            var tab = new CloseableTabItem();
            tab.SetHeader(header, tabName, ref tabControlItems, channel);
            tab.Content = tabInstance;
            tabControlItems.Add(tab);
        }

        public ICommand ControlPanelTabCommand => new DelegateCommand(() =>
        {
            addTab(new SettingsControl(), "Control Panel");
        });

        public ICommand CreateRoomTabCommand => new DelegateCommand(() =>
        {
            addTab(new CreateRoomControl(), "Create Room");
        });

        public ICommand SugestionTabCommand => new DelegateCommand(() =>
        {
            addTab(new SuggestControl(), "Suggestions");
        });

        public ICommand AbouseTabCommand => new DelegateCommand(() =>
        {
            addTab(new AbouseControl(), "Abouse");
        });

        public ICommand ArchiveTabCommand => new DelegateCommand(() =>
        {
            // TODO Whole
            // p_archive archive = new p_archive();
            //archive.Show();
        });

        public ICommand ContactTabCommand => new DelegateCommand(() =>
        {
            addTab(new ContactToAdminControl(), "Contact");
        });

        public ICommand AdminTabCommand => new DelegateCommand(() =>
        {
            addTab(new AdminControl(), "Admin Settings");
        });

        public ICommand InformationTabCommand => new DelegateCommand(() =>
        {
            addTab(new InformationControl(), "Information");
        });

        public ICommand LogoutCommand => new DelegateCommand(() =>
        {
            // Close here main window here somehow
            clientSendToServer.LogoutSend();

            //clientManager.config.SaveConfig(clientManager.config);// when user exit from program, we save configuration
        });

        #region ListBox Menu Items Buttons
        public ICommand AddFriendHandleCommand => new DelegateCommand(() =>
        {
            string friendName = selectedUser;
            if (usersConnected.Contains(friendName) && friendName != App.Client.strName)
                clientSendToServer.SendToServer(Command.manageFriend, "Add", friendName); // There is send information to server that i add someone to friend list
        });

        private void PrivateMessage(string usernName)
        {
            if (privateMessage == null)
            {
                privateMessage = new PrivateMessageWindow(usernName);
                privateMessage.Show();
            }
            else MessageBox.Show("You allready private talk with " + usernName, "Gold Chat: " + App.Client.strName, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public ICommand PrivateMsgToUserCommand => new DelegateCommand(() =>
        {
            string usernName = selectedUser;
            if (usersConnected.Contains(usernName) && usernName != App.Client.strName)
                PrivateMessage(usernName);
        });

        public ICommand IgnoreUserCommand => new DelegateCommand(() =>
        {
            string usernName = selectedUser;
            if (usersConnected.Contains(usernName) && usernName != App.Client.strName)
                clientSendToServer.SendToServer(Command.ignoreUser, "AddIgnore", usernName);
        });

        public ICommand PrivateMsgToFriendCommand => new DelegateCommand(() =>
        {
            string friendName = selectedFriendlyUser;

            if (friendlyUsersConnected.Contains(friendName) && usersConnected.Contains(friendName)) // Now if friend is in our friend list + his online(exists in clientList) 
                PrivateMessage(friendName);
            else MessageBox.Show("Your Friend " + friendName + " is offline", "Gold Chat: " + App.Client.strName, MessageBoxButton.OK, MessageBoxImage.Error);
        });

        public ICommand DeleteFriendCommand => new DelegateCommand(() =>
        {
            string friendName = selectedFriendlyUser;
            if (friendlyUsersConnected.Contains(friendName) && friendName != App.Client.strName)
                clientSendToServer.SendToServer(Command.manageFriend, "Delete", friendName);
        });

        public ICommand DeleteIgnoredUserCommand => new DelegateCommand(() =>
        {
            string ignoredUserName = selectedIgnoredUser;
            if (ignoredUsers.Contains(ignoredUserName))
                clientSendToServer.SendToServer(Command.ignoreUser, "DeleteIgnore", ignoredUserName);
        });

        public ICommand JoinToLobbieCommand => new DelegateCommand(() =>
        {
            string lobbieName = selectedLobbie;
            if (lobbies.Contains(lobbieName))
                clientSendToServer.SendToServer(Command.joinChannel, lobbieName);
        });

        public ICommand EnterToJoinedChannelCommand => new DelegateCommand(() =>
        {
            string joinedChannel = selectedJoinedLobbie;
            if (joinedChannelsList.Contains(joinedChannel))
                clientSendToServer.SendToServer(Command.enterChannel, joinedChannel);
        });

        public ICommand LeaveChannelCommand => new DelegateCommand(() =>
        {
            string channelName = selectedJoinedLobbie;
            if (joinedChannelsList.Contains(channelName))
                clientSendToServer.SendToServer(Command.leaveChannel, channelName);
        });

        public ICommand ExitChannelCommand => new DelegateCommand(() =>
        {
            string channelName = selectedJoinedLobbie;
            if (joinedChannelsList.Contains(channelName))
                clientSendToServer.SendToServer(Command.exitChannel, channelName);
        });

        public ICommand DeleteChannelCommand => new DelegateCommand(() =>
        {
            string channeName = selectedJoinedLobbie;
            if (joinedChannelsList.Contains(channeName))
            {
                //todo All think in client side (also show window to ask admin password to delete)
                //Data msgToSend = new Data();

                //msgToSend.strName = clientManager.userName;
                //msgToSend.strMessage = strMessage;
                //msgToSend.cmdCommand = Command.deleteChannel;

                //byte[] byteData = msgToSend.ToByte();
                //clientManager.BeginSend(byteData);
            }
        });
        #endregion

        #endregion

        private void OnClientLogin(object sender, ClientEventArgs e)
        {
            if (!usersConnected.Contains(e.clientLoginName))
                usersConnected.Add(e.clientLoginName);
        }

        private void OnClientLogout(object sender, ClientEventArgs e)
        {
            usersConnected.Remove(e.clientLogoutMessage);
        }

        private void OnClientList(object sender, ClientEventArgs e)
        {
            string[] splitNicks = e.clientListMessage.Split('*').Where(value => value != "").ToArray();
            foreach (string nick in splitNicks)
            {
                if (!usersConnected.Contains(nick))
                    usersConnected.Add(nick);
            }
        }

        private void OnClientPrivMessage(object sender, ClientEventArgs e)
        {
            if (privateMessage == null)
            {
                privateMessage = new PrivateMessageWindow(e.clientFriendName);
                privateMessage.Show();
            }
        }

        private void OnClientCreateChannel(object sender, ClientEventArgs e)
        {
            MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            if (e.clientChannelMsg2 == "CreatedChannel")
            {
                lobbies.Add(e.clientChannelName);
            }
        }
        /// todo ALL IN NEW CLASS
        //private void OnClientJoinChannel(object sender, ClientEventArgs e)
        //{
        //    if (e.clientChannelMsg == "Send Password" || e.clientChannelMsg == "Wrong Password")
        //    {
        //        MessageBox.Show("Send Password to channel " + e.clientChannelName, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);

        //        ServerAskClient serverAskClient = new ServerAskClient("Send Password", e.clientChannelName);
        //        serverAskClient.changeLabelContent("Send Password to channel " + e.clientChannelName);
        //        PasswordBox pb = serverAskClient.addPasswordBoxToWindow("pass1");
        //        pb.PreviewKeyDown += EnterClicked;
        //        Button button = serverAskClient.addButton("button", "send");
        //        button.Click += new RoutedEventHandler(button_Click);
        //        serverAskClient.Show();
        //    }
        //    else if (e.clientChannelMsg2 == "ChannelJoined" || e.clientChannelMsg2 == "CreatedChannel")
        //    {
        //        joinedChannelsList.Add(e.clientChannelName);
        //        MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //    else
        //        MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
        //}
        //void EnterClicked(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Return)
        //    {
        //        clientSendToServer.SendToServer(Command.joinChannel, ClientNameInChannel, clientSendToServer.CalculateChecksum(getMessage()));
        //        e.Handled = true;
        //    }
        //}
        //private void button_Click(object sender, RoutedEventArgs e)
        //{
        //    clientSendToServer.SendToServer(Command.joinChannel, ClientNameInChannel, clientSendToServer.CalculateChecksum(getMessage()));
        //}

        private void OnClientExitChannel(object sender, ClientEventArgs e)
        {
            if (e.clientChannelMsg == "You are exit from the channel")
            {
                MessageBox.Show(e.clientChannelMsg + " " + e.clientChannelName + ".", "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
                joinedChannelsList.Remove(e.clientChannelName);
            }
            else
                MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnClientListChannel(object sender, ClientEventArgs e)
        {
            string[] splitChannels = e.clientListChannelsMessage.Split('*').Where(value => value != "").ToArray();
            foreach (string channel in splitChannels)
            {
                if (!lobbies.Contains(channel))
                    lobbies.Add(channel);
            }
        }

        private void OnClientChannelEnter(object sender, ClientEventArgs e)
        {
            if (e.clientName == User.strName)
            {
                string channelName = e.clientChannelName;
                addTab(new ChannelControl(channelName, e.clientChannelMsg), channelName, true);

                // User enter to channel and want a list of all users in
                clientSendToServer.SendToServer(Command.List, "ChannelUsers", channelName);
            }
        }

        private void OnClientListChannelJoined(object sender, ClientEventArgs e)
        {
            string[] splitChannels = e.clientListChannelsMessage.Split('*').Where(value => value != "").ToArray(); ;

            foreach (string channel in splitChannels)
            {
                if (!joinedChannelsList.Contains(channel))
                    joinedChannelsList.Add(channel);
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

        private void OnClientAcceptFriend(object sender, ClientEventArgs e)
        {
            friendlyUsersConnected.Add(e.clientFriendName == User.strName ? e.clientName : e.clientFriendName);

            MessageBox.Show("You are now friend with: " + (e.clientFriendName == User.strName ? e.clientName : e.clientFriendName), "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnClientListFriends(object sender, ClientEventArgs e)
        {
            string[] listFriends = e.clientListFriendsMessage.Split('*').Where(value => value != "").ToArray(); ;
            foreach (string friendName in listFriends)
            {
                if (!friendlyUsersConnected.Contains(friendName))
                    friendlyUsersConnected.Add(friendName);
            }
        }

        //After we got msg from server that we/friend delete as/friend we need got list of friends
        private void OnClientDeleteFriend(object sender, ClientEventArgs e)
        {
            friendlyUsersConnected.Remove(e.clientFriendName == User.strName ? e.clientName : e.clientFriendName);
        }

        private void OnClientIgnoreUser(object sender, ClientEventArgs e)
        {
            MessageBox.Show(e.clientIgnoreMessage, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            if (e.clientIgnoreOption == "AddIgnore")
                ignoredUsers.Add(e.clientIgnoreName);
        }

        private void OnClientDeleteIgnoredUser(object sender, ClientEventArgs e)
        {
            MessageBox.Show(e.clientIgnoreMessage, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            if (e.clientIgnoreOption == "DeleteIgnore")
            {
                ignoredUsers.Remove(e.clientIgnoreName);
                clientSendToServer.SendToServer(Command.List);
            }
        }

        private void OnClientListIgnored(object sender, ClientEventArgs e)
        {
            string[] ignoredUsersFromServer = e.clientListFriendsMessage.Split('*').Where(value => value != "").ToArray(); ;
            foreach (string name in ignoredUsersFromServer)
            {
                if (!ignoredUsers.Contains(name))
                    ignoredUsers.Add(name);
            }
        }

        private void OnClientKickFromSerwer(object sender, ClientEventArgs e)
        {
            if (e.clientName == User.strName)
            {
                MessageBox.Show("You are kicked reason: " + e.clientKickReason, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
                //Close();
            }
            else
            {
                usersConnected.Remove(e.clientName);
                //incomeMsg += e.clientName + " has kicked reason: " + e.clientKickReason + "\r\n";
            }
        }

        private void OnClientBanFromSerwer(object sender, ClientEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
