using CommandClient;
using Gold_Client.Model;
using Gold_Client.ProgramableWindow;
using Gold_Client.View;
using Gold_Client.View.Others;
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

        public Action CloseAction;

        /// HACK -> send list of tab items to CloseableTabItem and there remove TabItem
        private/* readonly*/ ObservableCollection<object> tabControlItems = new ObservableCollection<object>();
        //public IEnumerable<object> TabControlItems => tabControlItems;
        public ObservableCollection<object> TabControlItems { get { return tabControlItems; } }

        public int selectedTabControlIndex = 0;

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

        public MainContentPresenter(Action closeAction)
        {
            User = App.Client;

            CloseAction = closeAction;

            proccesReceiverInformation.ClientLogin += OnClientLogin;
            proccesReceiverInformation.ClientLogout += OnClientLogout;
            proccesReceiverInformation.ClientList += OnClientList;

            clientReceiveFromServer.ReceiveLogExcep += (s, e) => MessageBox.Show(e.receiveLogExpceMessage, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Error);
            //proccesReceiverInformation.SendException += (s, e) => MessageBox.Show(e.sendExcepMessage, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Error);
            //channels
            proccesReceiverInformation.ClientCreateChannel += OnClientCreateChannel;
            proccesReceiverInformation.ClientDeleteChannel += OnClientDeleteChannel;
            proccesReceiverInformation.ClientEditChannel += (s, e) => MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            proccesReceiverInformation.ClientJoinChannel += OnClientJoinChannel; // all in new class
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
            proccesReceiverInformation.ClientKickFromServer += OnClientKickFromServer;
            proccesReceiverInformation.ClientBanFromServer += OnClientBanFromServer;
            proccesReceiverInformation.ClientReceiveFileInfo += OnClientReceiveFileInfo;
            InformServerToSendUserLists informServerToSendUserLists = new InformServerToSendUserLists();

            addTab(new GlobalMessageContent(), "Main");
            SelectedTabControlIndex = 0;
        }

        private void OnClientReceiveFileInfo(object sender, ClientEventArgs e)
        {
            if (e.clientName == App.Client.strName && e.FileLen != "AcceptReceive")
            {
                ReceiveFileWindow getFileInfo = new ReceiveFileWindow(e.clientFriendName, e.FileName, Convert.ToInt64(e.FileLen));
                getFileInfo.Show();
            }
        }

        public Client User
        {
            get; set;
        }

        public int SelectedTabControlIndex
        {
            get { return selectedTabControlIndex; }

            set
            {
                selectedTabControlIndex = value;
                RaisePropertyChangedEvent(nameof(SelectedTabControlIndex));
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

        private bool background;
        public bool FriendLoginColor
        {
            get { return background; }
            set
            {
                background = value;
                RaisePropertyChangedEvent(nameof(FriendLoginColor));
            }
        }

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
            clientSendToServer.SendToServer(Command.Logout);

            //clientManager.config.SaveConfig(clientManager.config);// when user exit from program, we save configuration
        });

        #region ListBox Menu Items Buttons
        public ICommand AddFriendHandleCommand => new DelegateCommand(() =>
        {
            if (usersConnected.Contains(selectedUser) && selectedUser != App.Client.strName)
                clientSendToServer.SendToServer(Command.manageFriend, "Add", selectedUser); // There is send information to server that i add someone to friend list
        });

        private void PrivateMessage(string usernName)
        {
            bool? isPrivateWindowClosed = false;
            if (privateMessage == null)
            {
                privateMessage = new PrivateMessageWindow(usernName);
                isPrivateWindowClosed = privateMessage.ShowDialog();
            }
            else MessageBox.Show("You allready private talk with " + usernName, "Gold Chat: " + App.Client.strName, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public ICommand PrivateMsgToUserCommand => new DelegateCommand(() =>
        {
            if (usersConnected.Contains(selectedUser) && selectedUser != App.Client.strName)
                PrivateMessage(selectedUser);
        });

        public ICommand IgnoreUserCommand => new DelegateCommand(() =>
        {
            if (usersConnected.Contains(selectedUser) && selectedUser != App.Client.strName)
                clientSendToServer.SendToServer(Command.ignoreUser, "AddIgnore", selectedUser);
        });

        public ICommand BanUserCommand => new DelegateCommand(() =>
        {
            if (usersConnected.Contains(selectedUser) && selectedUser != App.Client.strName)
            {
                BanUserWindow banWindow = new BanUserWindow(selectedUser, null);
                banWindow.Show();
            }
        });

        public ICommand KickUserCommand => new DelegateCommand(() =>
        {
            if (usersConnected.Contains(selectedUser) && selectedUser != App.Client.strName)
            {
                GenerateTexBoxWindow createTextWindow = new GenerateTexBoxWindow();
                ChannelKickUserReason.UserName = selectedUser;
                createTextWindow.OnClickOrEnter += ChannelKickUserReason.OnClickOrEnter;
                createTextWindow.createWindow("Kick: " + selectedUser + " reason", "Give reason of kicking: " + selectedUser);
            }
        });

        public ICommand PrivateMsgToFriendCommand => new DelegateCommand(() =>
        {
            string friendName = selectedFriendlyUser;

            if (friendlyUsersConnected.Contains(friendName) && usersConnected.Contains(friendName)) // Now if friend is in our friend list + his online(exists in clientList) 
                PrivateMessage(friendName);
            else MessageBox.Show("Your Friend " + friendName + " is offline", "Gold Chat: " + App.Client.strName, MessageBoxButton.OK, MessageBoxImage.Error);
        });

        public ICommand SendFileCommand => new DelegateCommand(() =>
        {
            SendFileWindow sendFileWindow = new SendFileWindow(selectedFriendlyUser);
            sendFileWindow.Show();
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
            if (lobbies.Contains(lobbieName) && !joinedChannelsList.Contains(lobbieName))
                clientSendToServer.SendToServer(Command.joinChannel, lobbieName);
        });

        public ICommand EnterToJoinedChannelCommand => new DelegateCommand(() =>
        {
            string joinedChannel = selectedJoinedLobbie;
            if (joinedChannelsList.Contains(joinedChannel))
                clientSendToServer.SendToServer(Command.enterChannel, joinedChannel);

            SelectedTabControlIndex = GetLastTabControlElement();
        });

        // Can do some function in CloseableTabItem to get tab name and use that name to select index
        private int GetLastTabControlElement()
        {
            int index = 0;
            foreach (var closeTableItem in tabControlItems)
            {
                index++;
            }
            return index;
        }

        public ICommand LeaveChannelCommand => new DelegateCommand(() =>
        {
            string channelName = selectedJoinedLobbie;
            if (joinedChannelsList.Contains(channelName))
                clientSendToServer.SendToServer(Command.leaveChannel, channelName);
        });

        public ICommand ExitChannelCommand => new DelegateCommand(() =>
        {
            string channelName = selectedJoinedLobbie;
            if (joinedChannelsList.Contains(channelName)) //Todo messagebox yes|no exic cuse next time user need to post password
                clientSendToServer.SendToServer(Command.exitChannel, channelName);
        });

        public ICommand DeleteChannelCommand => new DelegateCommand(() =>
        {
            string channeName = selectedJoinedLobbie;
            if (joinedChannelsList.Contains(channeName))
            {
                GenerateChannelPassWindow sendPassword = new GenerateChannelPassWindow();
                DeleteChannelSend.ChannelName = channeName;
                sendPassword.OnClickOrEnter += DeleteChannelSend.OnClickOrEnter;
                sendPassword.createWindow("Send Password: " + channeName, "Send Password to delete " + channeName);
            }
        });
        #endregion

        #endregion

        #region Selected Lists Headers
        public string AddFriendHeaderCommand
        {
            get { return "Add " + selectedUser + " to friend list"; }
        }

        public string AddToIgnoreHeaderCommand
        {
            get { return "Add " + selectedUser + " to ignored list"; }
        }

        public string BanUserHeaderCommand
        {
            get { return "Ban " + selectedUser; }
        }

        public string KickUserHeaderCommand
        {
            get { return "Kick " + selectedUser; }
        }

        public string DeleteFriendHeaderCommand
        {
            get { return "Delete " + selectedFriendlyUser + " from friend list?"; }
        }

        public string PrivMsgHeaderCommand
        {
            get { return "Send private message to " + selectedFriendlyUser; }
        }

        public string SendFileHeaderCommand
        {
            get { return "Send file to " + selectedFriendlyUser; }
        }

        public string DeleteIgnoredHeaderCommand
        {
            get { return "Delete " + selectedIgnoredUser + " from ignored list?"; }
        }

        public string JoinToLobbieHeaderCommand
        {
            get { return "Join to " + selectedLobbie; }
        }

        public string EnterToLobbieHeaderCommand
        {
            get { return "Enter to " + selectedJoinedLobbie; }
        }

        public string LeaveLobbieHeaderCommand
        {
            get { return "Leave from " + selectedJoinedLobbie + "?"; }
        }

        public string ExitLobbieHeaderCommand
        {
            get { return "Exit from" + selectedJoinedLobbie + "?"; }
        }

        public string DeleteToLobbieHeaderCommand
        {
            get { return "Delete channel " + selectedJoinedLobbie + "?"; }
        }
        #endregion

        private void OnClientLogin(object sender, ClientEventArgs e)
        {
            if (!usersConnected.Contains(e.clientLoginName))
            {
                usersConnected.Add(e.clientLoginName);
                if (friendlyUsersConnected.Contains(e.clientLoginName))
                {
                    FriendLoginColor = true;
                }
            }
        }

        private void OnClientLogout(object sender, ClientEventArgs e)
        {
            usersConnected.Remove(e.clientLogoutMessage);
        }

        private void OnClientList(object sender, ClientEventArgs e)
        {
            if (e.clientListMessage != null)
            {
                string[] splitNicks = e.clientListMessage.Split('*').Where(value => value != "").ToArray();
                foreach (string nick in splitNicks)
                {
                    if (!usersConnected.Contains(nick))
                        usersConnected.Add(nick);
                }
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

        private void OnClientCreateChannel(object sender, ClientEventArgs e)
        {
            if (e.clientChannelMsg2 == "CreatedChannel")
                lobbies.Add(e.clientChannelMsg);
            if (e.clientName == App.Client.strName)
                joinedChannelsList.Add(e.clientChannelMsg);
        }

        private void OnClientJoinChannel(object sender, ClientEventArgs e)
        {
            if (e.clientChannelMsg == "Send Password" || e.clientChannelMsg == "Wrong Password")
            {
                GenerateChannelPassWindow sendPassword = new GenerateChannelPassWindow();
                JoinChannelSend.ChannelName = e.clientChannelName;
                sendPassword.OnClickOrEnter += JoinChannelSend.OnClickOrEnter;
                sendPassword.createWindow("Send Password: " + e.clientChannelName, "Send Password to channel " + e.clientChannelName + " for join");
            }
            else if (e.clientChannelMsg2 == "ChannelJoined" || e.clientChannelMsg2 == "CreatedChannel" && joinedChannelsList.Contains(e.clientChannelName))
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
                {
                    friendlyUsersConnected.Add(friendName);
                    if (!usersConnected.Contains(friendName))
                        FriendLoginColor = false;
                }
            }
        }

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

        private void OnClientKickFromServer(object sender, ClientEventArgs e)
        {
            if (e.clientName == User.strName)
            {
                MessageBox.Show("You are" + e.clientKickReason, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction();
            }
            else
                usersConnected.Remove(e.clientName);
        }

        private void OnClientBanFromServer(object sender, ClientEventArgs e)
        {
            if (e.clientName != User.strName)
            {
                if (usersConnected.Contains(e.clientName))
                    usersConnected.Remove(e.clientName);
                if (friendlyUsersConnected.Contains(e.clientName))
                    friendlyUsersConnected.Remove(e.clientName);
            }
            else if (e.clientName == User.strName)
            {
                MessageBox.Show(e.clientBanReason, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Stop);
                CloseAction();
            }

        }

        private void OnClientDeleteChannel(object sender, ClientEventArgs e)
        {
            if (e.clientChannelMsg2 != "Deny")
            {
                if (Lobbies.Contains(e.clientChannelMsg))
                    lobbies.Remove(e.clientChannelMsg);
                if (JoinedChannelsList.Contains(e.clientChannelMsg))
                    joinedChannelsList.Remove(e.clientChannelMsg);
                MessageBox.Show(e.clientChannelMsg2 + " deleted channel " + e.clientChannelMsg, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
                MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Stop);
        }
    }
}
