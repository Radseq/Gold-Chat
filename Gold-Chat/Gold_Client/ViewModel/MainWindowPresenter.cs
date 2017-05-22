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
    public class MainWindowPresenter : ObservableObject, IClient
    {
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;
        ClientReceivedFromServer clientReceiveFromServer = ClientReceivedFromServer.Instance;
        ProcessReceivedByte proccesReceiverInformation = new ProcessReceivedByte();

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

        public MainWindowPresenter()
        {
            proccesReceiverInformation.ProccesBuffer();
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
            proccesReceiverInformation.ClientJoinChannel += OnClientJoinChannel;
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
            // InformServerToSendUserLists informServerToSendUserLists = new InformServerToSendUserLists();

            MainContent mc = new MainContent();
            var header = new TextBlock { Text = "Main" };

            var controlPanelTab = new CloseableTabItem();
            controlPanelTab.SetHeader(header, "Main", ref tabControlItems);
            controlPanelTab.Content = mc;

            tabControlItems.Add(controlPanelTab);
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
        public ICommand ControlPanelTabCommand => new DelegateCommand(() =>
        {
            SettingsControl ctrl_panel = new SettingsControl();

            var header = new TextBlock { Text = "Control Panel" };

            var controlPanelTab = new CloseableTabItem();
            controlPanelTab.SetHeader(header, "Control Panel", ref tabControlItems);
            controlPanelTab.Content = ctrl_panel;

            //TabItem ti = new TabItem();
            //ti.Header = "Control Panel";
            //ti.Content = ctrl_panel;
            tabControlItems.Add(controlPanelTab);
        });

        public ICommand CreateRoomTabCommand => new DelegateCommand(() =>
        {
            CreateRoomControl create_room = new CreateRoomControl();

            var header = new TextBlock { Text = "Create Room" };

            var createRoomTab = new CloseableTabItem();
            createRoomTab.SetHeader(header, "Create Room", ref tabControlItems);
            createRoomTab.Content = create_room;

            tabControlItems.Add(createRoomTab);
        });

        public ICommand SugestionTabCommand => new DelegateCommand(() =>
        {
            SuggestControl suggest = new SuggestControl();
            var header = new TextBlock { Text = "Suggestions" };

            var suggestionTab = new CloseableTabItem();
            suggestionTab.SetHeader(header, "Suggestions", ref tabControlItems);
            suggestionTab.Content = suggest;

            tabControlItems.Add(suggestionTab);
        });

        public ICommand AbouseTabCommand => new DelegateCommand(() =>
        {
            AbouseControl abouse = new AbouseControl();

            var header = new TextBlock { Text = "Abouse" };

            var abouseTab = new CloseableTabItem();
            abouseTab.SetHeader(header, "Abouse", ref tabControlItems);
            abouseTab.Content = abouse;

            tabControlItems.Add(abouseTab);
        });

        public ICommand ArchiveTabCommand => new DelegateCommand(() =>
        {
            // TODO Whole
            // p_archive archive = new p_archive();
            //archive.Show();
        });

        public ICommand ContactTabCommand => new DelegateCommand(() =>
        {
            ContactToAdminControl contact = new ContactToAdminControl();

            var header = new TextBlock { Text = "Contact" };

            var contactTab = new CloseableTabItem();
            contactTab.SetHeader(header, "Contact", ref tabControlItems);
            contactTab.Content = contact;

            tabControlItems.Add(contactTab);
        });

        public ICommand AdminTabCommand => new DelegateCommand(() =>
        {
            AdminControl adm_settings = new AdminControl();

            var header = new TextBlock { Text = "Admin Settings" };

            var adminTab = new CloseableTabItem();
            adminTab.SetHeader(header, "Admin Settings", ref tabControlItems);
            adminTab.Content = adm_settings;

            tabControlItems.Add(adminTab);
        });

        public ICommand InformationTabCommand => new DelegateCommand(() =>
        {
            InformationControl inform = new InformationControl();
            var header = new TextBlock { Text = "Information" };

            var informationTab = new CloseableTabItem();
            informationTab.SetHeader(header, "Information", ref tabControlItems);
            informationTab.Content = inform;

            tabControlItems.Add(informationTab);
        });

        public ICommand LogoutCommand => new DelegateCommand(() =>
        {
            // Close here main window here somehow
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
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
            PrivateMessageWindow pmWindow = new PrivateMessageWindow(usernName);
            //pm = new private_message(usernName);
            //pm.Show();
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
                {
                    //// still nothink
                    //var uiContext = SynchronizationContext.Current;
                    //uiContext.Send(x => usersConnected.Add(nick), null);

                    //Application.Current.Dispatcher.Invoke(delegate
                    //{
                    usersConnected.Add(nick);
                    //});
                }
            }
            //clientList.AddRange(e.clientListMessage.Split('*'));
            usersConnected.RemoveAt(usersConnected.Count - 1);

        }

        private void OnClientPrivMessage(object sender, ClientEventArgs e)
        {
            PrivateMessageWindow privateMessage = new PrivateMessageWindow(e.clientFriendName);
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
            MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            if (e.clientChannelMsg2 == "CreatedChannel")
            {
                lobbies.Add(e.clientChannelName);
            }
        }

        private void OnClientJoinChannel(object sender, ClientEventArgs e)
        {
            if (e.clientChannelMsg == "Send Password" || e.clientChannelMsg == "Wrong Password")
            {
                MessageBox.Show("Send Password to channel " + e.clientChannelName, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);

                ServerAskClient serverAskClient = new ServerAskClient("Send Password", e.clientChannelName);
                serverAskClient.changeLabelContent("Send Password to channel " + e.clientChannelName);
                serverAskClient.addPasswordBoxToWindow("pass1");
                serverAskClient.addButton("button", "send");
                serverAskClient.Show();
            }
            else if (e.clientChannelMsg2 == "ChannelJoined" || e.clientChannelMsg2 == "CreatedChannel")
            {
                joinedChannelsList.Add(e.clientChannelName);
                MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
                MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
        }

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
            //string[] splitChannels = e.clientListChannelsMessage.Split('*');
            string[] splitChannels = e.clientListChannelsMessage.Split('*').Where(value => value != "").ToArray();
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
            if (e.clientName == User.strName)
            {
                string channelName = e.clientChannelName;
                ChannelControl channelControl = new ChannelControl(channelName, e.clientChannelMsg);

                var header = new TextBlock { Text = channelName };

                var channelTab = new CloseableTabItem();
                channelTab.SetHeader(header, channelName, ref tabControlItems);
                channelTab.Content = channelControl;

                tabControlItems.Add(channelTab);

                ////user enter to channel and want a list of all users in
                clientSendToServer.SendToServer(Command.List, "ChannelUsers", channelName);

                //lets print motd 
                //channelPanel.channelMessages.Text += "<<< Welcome Message: " + e.clientChannelMsg + " >>>>" + "\r\n";
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
            //joinedChannelsList.AddRange(e.clientListChannelsMessage.Split('*'));
            //joinedChannelsList.RemoveAt(clientChannelsJoinedList.Count - 1);
            //lbJoinedChann.ItemsSource = clientChannelsJoinedList;
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
