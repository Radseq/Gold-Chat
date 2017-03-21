//using Server;
using CommandClient;
using System;
using System.Collections;
using System.ComponentModel;
//chat
//using Gold.Utils;
//voice
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Gold
{

    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class program : Window
    {
        public static string strName = App.clientName;          //Name by which the user logs into the room

        ArrayList clientList = new ArrayList();
        ArrayList clientIgnoredList = new ArrayList();         //list of ignored users
        ArrayList clientChannelsList = new ArrayList();         //list of all channels
        ArrayList clientChannelsJoinedList = new ArrayList();   //list of channels that i joined
        ArrayList clientFriendsList = new ArrayList();          //list of friends thats i add

        public static private_message pm; //private message window

        private System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        private static ClientManager clientManager;

        //tab_windows.channelWindow channelPanel = null;  //user channel window

        int second = 0;

        public program(ClientManager cm)
        {
            InitializeComponent();
            clientManager = cm;
            clientManager.ClientLogin += OnClientLogin;
            clientManager.ClientLogout += OnClientLogout;
            clientManager.ClientList += OnClientList;
            clientManager.ClientMessage += OnClientMessage;
            clientManager.ClientPrivMessage += OnClientPrivMessage;
            clientManager.ClientChangePass += (s, e) => MessageBox.Show(e.clientChangePassMessage, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Information);
            //clientManager.ReceiveLogExcep += OnReceiveLogExcep;
            //clientManager.SendException += OnSendException;
            //OR by lambda - dont work with unsubscribers
            // or use that EventHandler handler = (s, e) => MessageBox.Show("Woho"); then += handler; or -= handler;
            clientManager.ReceiveLogExcep += (s, e) => MessageBox.Show(e.receiveLogExpceMessage, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Error);
            clientManager.SendException += (s, e) => MessageBox.Show(e.sendExcepMessage, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Error);
            //channels
            clientManager.ClientCreateChannel += OnClientCreateChannel;
            clientManager.ClientDeleteChannel += OnClientDeleteChannel;
            clientManager.ClientEditChannel += OnClientEditChannel;
            clientManager.ClientJoinChannel += OnClientJoinChannel;
            clientManager.ClientExitChannel += OnClientExitChannel;
            clientManager.ClientListChannel += OnClientListChannel;
            clientManager.ClientChannelEnter += OnClientChannelEnter;
            clientManager.ClientChannelEnterDeny += OnClientChannelEnterDeny;
            clientManager.ClientListChannelJoined += OnClientListChannelJoined;
            //friends
            clientManager.ClientAddFriend += OnClientAddFriend;
            clientManager.ClientAcceptFriend += OnClientAcceptFriend;
            clientManager.ClientListFriends += OnClientListFriends;
            clientManager.ClientDeleteFriend += OnClientDeleteFriend;
            clientManager.ClientDenyFriend += OnClientDenyFriend;
            clientManager.ClientIgnoreUser += OnClientIgnoreUser;
            clientManager.ClientDeleteIgnoredUser += OnClientDeleteIgnoredUser;
            clientManager.ClientListIgnored += OnClientListIgnored;
            //ban/kick
            clientManager.ClientKickFromSerwer += OnClientKickFromSerwer;
            clientManager.ClientBanFromSerwer += OnClientBanFromSerwer;

            getClientList();

            dispatcherTimer.Tick += new EventHandler(getFromServLists);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
        }

        private void OnClientBanFromSerwer(object sender, ClientEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnClientKickFromSerwer(object sender, ClientEventArgs e)
        {
            if (e.clientName == App.clientName)
            {
                MessageBox.Show("You are kicked reason: " + e.clientKickReason, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            else
            {
                clientList.Remove(e.clientName);
                lb_users.Items.Refresh();
                txtChatBox.Text += e.clientName + " has kicked reason: " + e.clientKickReason + "\r\n";
            }
        }

        private void getClientList()
        {
            clientManager.SendToServer(Command.List);
        }

        private void OnClientChannelEnterDeny(object sender, ClientEventArgs e)
        {
            MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void getFromServLists(object sender, EventArgs e)
        {
            // ???
            if (second == 0)
            {
                //The user has logged into the system so we now request the server to send
                //the names of all users who are in the chat room

            }
            else if (second == 1)
                clientManager.SendToServer(Command.List, "IgnoredUsers"); // The names of all ignored users
            else if (second == 2)
                clientManager.SendToServer(Command.List, "ChannelsJoined"); // The names of all channels that he have joined
            else if (second == 3)
                clientManager.SendToServer(Command.List, "Friends"); // The names of all users that he have in friend list
            else if (second == 4)
                clientManager.SendToServer(Command.List, "Channel"); // The names of all channels that he have acces to
            if (second > 4)
                dispatcherTimer.Stop();
            second = second + 1;
        }

        //private void getFromServLists()
        //{
        //    //The user has logged into the system so we now request the server to send
        //    //the names of all users who are in the chat room
        //    Data msgToSend = new Data();
        //    msgToSend.cmdCommand = Command.List;
        //    msgToSend.strName = strName;
        //    msgToSend.strMessage = null;

        //    byteData = msgToSend.ToByte();

        //    clientManager.BeginSend(byteData);

        //    //the names of all ignored users
        //    Data msgToSendIgnoredUsers = new Data();
        //    msgToSendIgnoredUsers.cmdCommand = Command.List;
        //    msgToSendIgnoredUsers.strName = strName;
        //    msgToSendIgnoredUsers.strMessage = "IgnoredUsers";

        //    byteData = msgToSendIgnoredUsers.ToByte();

        //    clientManager.BeginSend(byteData);

        //    //the names of all channels that he have joined
        //    Data msgToSendJoinedChannels = new Data();
        //    msgToSendJoinedChannels.cmdCommand = Command.List;
        //    msgToSendJoinedChannels.strName = strName;
        //    msgToSendJoinedChannels.strMessage = "ChannelsJoined";

        //    byteData = msgToSendJoinedChannels.ToByte();

        //    clientManager.BeginSend(byteData);
        //}

        private void OnClientListIgnored(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                clientIgnoredList.AddRange(e.clientListMessage.Split('*'));
                clientIgnoredList.RemoveAt(clientIgnoredList.Count - 1);
                lb_ignored.ItemsSource = clientIgnoredList;
                lb_ignored.Items.Refresh();
            }));
        }

        private void OnClientDeleteIgnoredUser(object sender, ClientEventArgs e)
        {
            MessageBox.Show(e.clientIgnoreMessage, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Information);
            if (e.clientIgnoreOption == "DeleteIgnore")
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    clientIgnoredList.Remove(e.clientIgnoreName);
                    lb_ignored.Items.Remove(e.clientIgnoreName);
                    lb_ignored.Items.Refresh();
                    getClientList();
                }));
            }
        }

        private void OnClientIgnoreUser(object sender, ClientEventArgs e)
        {
            MessageBox.Show(e.clientIgnoreMessage, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Information);
            if (e.clientIgnoreOption == "AddIgnore")
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    clientIgnoredList.Add(e.clientIgnoreName);
                    lb_ignored.Items.Add(e.clientIgnoreName);
                    lb_ignored.Items.Refresh();
                }));
            }
        }

        private void OnClientDenyFriend(object sender, ClientEventArgs e)
        {
            MessageBox.Show("User: " + e.clientFriendName + " doesnt accept your ask to be your friend", "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnClientListChannelJoined(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                clientChannelsJoinedList.AddRange(e.clientListChannelsMessage.Split('*'));
                clientChannelsJoinedList.RemoveAt(clientChannelsJoinedList.Count - 1);
                lbJoinedChann.ItemsSource = clientChannelsJoinedList;
            }));
        }

        private void OnClientChannelEnter(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (e.clientName == App.clientName)
                {
                    string channelName = e.clientChannelName;
                    tab_windows.channelWindow channelPanel = new tab_windows.channelWindow(clientManager, channelName);

                    var header = new TextBlock { Text = channelName };
                    // Create the tab
                    var tab = new CloseableTabItem(clientManager);
                    tab.SetHeader(header, channelName, true);
                    tab.Content = channelPanel;

                    // Add to TabControl
                    tc.Items.Add(tab);

                    //user enter to channel and want a list of all users in
                    clientManager.SendToServer(Command.List, "ChannelUsers", channelName);

                    //lets print motd 
                    channelPanel.channelMessages.Text += "<<< Welcome Message: " + e.clientChannelMsg + " >>>>" + "\r\n";
                }

            }));
        }

        //after we got msg from server that we/friend delete as/friend we need got list of friends
        private void OnClientDeleteFriend(object sender, ClientEventArgs e)
        {
            //the names of all users that he have in friend list
            //Data msgToSendFriends = new Data();
            //msgToSendFriends.cmdCommand = Command.List;
            //msgToSendFriends.strName = strName;
            //msgToSendFriends.strMessage = "Friends";

            //byteData = msgToSendFriends.ToByte();

            //clientManager.BeginSend(byteData);
            Dispatcher.BeginInvoke((Action)(() =>
            {
                clientFriendsList.Remove(e.clientFriendName == App.clientName ? e.clientName : e.clientFriendName);
                lb_friend_users.Items.Remove(e.clientFriendName == App.clientName ? e.clientName : e.clientFriendName); //change in server side: send respoand and msg to nick
                lb_friend_users.Items.Refresh();
            }));
        }

        private void OnClientListFriends(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                clientFriendsList.AddRange(e.clientListFriendsMessage.Split('*'));
                clientFriendsList.RemoveAt(clientFriendsList.Count - 1);
                lb_friend_users.ItemsSource = clientFriendsList;
                lb_friend_users.Items.Refresh();
            }));
        }

        private void OnClientAcceptFriend(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                clientFriendsList.Add(e.clientFriendName == App.clientName ? e.clientName : e.clientFriendName);//change in server side: send respoand and msg to nick

                lb_friend_users.Items.Add(e.clientFriendName == App.clientName ? e.clientName : e.clientFriendName);//change in server side: send respoand and msg to nick
                lb_friend_users.Items.Refresh();
            }));//change in server side: send respoand and msg to nick
            MessageBox.Show("You are now friend with: " + (e.clientFriendName == App.clientName ? e.clientName : e.clientFriendName), "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnClientAddFriend(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                MessageBoxResult result = MessageBox.Show("User: " + e.clientFriendName + " want to be your friend Accept?", "Gold Chat: " + strName, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                    clientManager.SendToServer(Command.manageFriend, "No", e.clientFriendName);
                else
                    clientManager.SendToServer(Command.manageFriend, "Yes", e.clientFriendName);
            }));
        }

        private void OnClientListChannel(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {

                string[] splitChannels = e.clientListChannelsMessage.Split('*');
                foreach (string channel in splitChannels)
                {
                    if (!clientChannelsList.Contains(channel))
                        clientChannelsList.Add(channel);
                }

                //clientChannelsList.AddRange(e.clientListChannelsMessage.Split('*'));
                clientChannelsList.RemoveAt(clientChannelsList.Count - 1);
                lbLobbies.ItemsSource = clientChannelsList;
                lbLobbies.Items.Refresh();
            }));
        }

        private void OnClientJoinChannel(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (e.clientChannelMsg == "Send Password" || e.clientChannelMsg == "Wrong Password")
                {
                    //i need programmatically create window
                    MessageBox.Show("Send Password to channel " + e.clientChannelName, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Information);

                    serverAsk sa = new serverAsk(clientManager, e.clientChannelName, e.clientChannelMsg);
                    sa.Show();
                }
                else if (e.clientChannelMsg2 == "ChannelJoined" || e.clientChannelMsg2 == "CreatedChannel")
                {
                    clientChannelsJoinedList.Add(e.clientChannelName);
                    lbJoinedChann.Items.Refresh();
                    MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                //else if (e.clientChannelMsg2 == "CreatedChannel")
                //{
                //    MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Information);
                //}
                else
                {
                    MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }));
        }

        private void OnClientExitChannel(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                clientChannelsJoinedList.Remove(e.clientChannelName);
                lbJoinedChann.Items.Refresh();

            }));
            MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnClientEditChannel(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Information);
            }));
        }

        private void OnClientDeleteChannel(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Information);
            }));
        }

        private void OnClientCreateChannel(object sender, ClientEventArgs e)
        {
            MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Information);
            if (e.clientChannelMsg2 == "CreatedChannel")
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    clientChannelsJoinedList.Add(e.clientChannelName);
                    lbJoinedChann.Items.Refresh();
                }));
            }
        }

        private void OnClientLogin(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                //if (!clientList.Contains(e.clientLoginName))
                //{
                clientList.Add(e.clientLoginName);
                lb_users.Items.Refresh();
                txtChatBox.Text += e.clientLoginMessage + "\r\n";
                //}
            }));
        }

        /*
        private void OnSendException(object sender, ClientEventArgs e)
        {
        MessageBox.Show(e.sendExcepMessage, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnReceiveLogExcep(object sender, ClientEventArgs e)
        {
        MessageBox.Show(e.receiveLogExpceMessage, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Error);
        }*/

        private void OnClientPrivMessage(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (pm != null)
                {
                    pm.showPrivMessageTb.Text += e.clientFriendName + ": " + e.clientPrivMessage;
                }
                else
                {
                    pm = new private_message(e.clientFriendName);
                    pm.Show();
                    pm.showPrivMessageTb.Text += e.clientFriendName + ": " + e.clientPrivMessage;
                }
            }));
        }

        private void OnClientMessage(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                txtChatBox.Text += e.clientMessage;
            }));
        }

        private void OnClientList(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                string[] splitNicks = e.clientListMessage.Split('*');
                foreach (string nick in splitNicks)
                {
                    if (!clientList.Contains(nick))
                        clientList.Add(nick);
                }
                //clientList.AddRange(e.clientListMessage.Split('*'));
                clientList.RemoveAt(clientList.Count - 1);
                lb_users.ItemsSource = clientList;
                lb_users.Items.Refresh();
            }));
        }

        private void OnClientLogout(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                clientList.Remove(e.clientLogoutMessage);
                txtChatBox.Text += "<<<" + e.clientLogoutMessage + " has left the room>>>" + "\r\n";
                lb_users.Items.Refresh();
            }));
        }

        private void SendMessage()
        {
            try
            {
                clientManager.SendToServer(Command.Message, tb_message.Text);

                tb_message.Text = null;
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to send message to the server.", "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void SendPrivMessage()
        {
            try
            {
                clientManager.SendToServer(Command.privMessage, pm.sendPrivMessageTb.Text, pm.strMessage);
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to send message to the server.", "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void EnterClicked(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (!string.IsNullOrEmpty(tb_message.Text))
                {
                    SendMessage();
                    tb_message.Clear();
                }
                else
                    MessageBox.Show("Please insert a text", "Error validation", MessageBoxButton.OK, MessageBoxImage.Information);
                e.Handled = true;
            }
        }

        #region --- tabs ---
        private void control_panel_button_Click(object sender, RoutedEventArgs e)
        {
            tab_windows.p_user ctrl_panel = new tab_windows.p_user(clientManager);

            var header = new TextBlock { Text = "Control Panel" };
            // Create the tab
            var tab = new CloseableTabItem(clientManager);
            tab.SetHeader(header, "Control Panel");
            tab.Content = ctrl_panel;

            // Add to TabControl
            tc.Items.Add(tab);
        }

        private void create_room_buttom_Click(object sender, RoutedEventArgs e)
        {
            tab_windows.p_create_room create_room = new tab_windows.p_create_room(clientManager);

            var header = new TextBlock { Text = "Create Room" };

            // Create the tab
            var tab = new CloseableTabItem(clientManager);
            tab.SetHeader(header, "Create Room");
            tab.Content = create_room;

            // Add to TabControl
            tc.Items.Add(tab);
        }

        private void suggest_buttom_Click(object sender, RoutedEventArgs e)
        {
            tab_windows.p_suggest suggest = new tab_windows.p_suggest();
            var header = new TextBlock { Text = "Suggestions" };

            // Create the tab
            var tab = new CloseableTabItem(clientManager);
            tab.SetHeader(header, "Suggestions");
            tab.Content = suggest;

            // Add to TabControl
            tc.Items.Add(tab);
        }

        private void archive_Click(object sender, RoutedEventArgs e)
        {
            tab_windows.p_archive archive = new tab_windows.p_archive();
            //tc.Items.Add(suggest);
            archive.Show();
        }


        private void abouse_buttom_Click(object sender, RoutedEventArgs e)
        {
            tab_windows.p_abouse abouse = new tab_windows.p_abouse();


            var header = new TextBlock { Text = "Abouse" };

            // Create the tab
            var tab = new CloseableTabItem(clientManager);
            tab.SetHeader(header, "Abouse");
            tab.Content = abouse;

            // Add to TabControl
            tc.Items.Add(tab);
        }

        private void contact_buttom_Click(object sender, RoutedEventArgs e)
        {
            tab_windows.p_contact contact = new tab_windows.p_contact();

            var header = new TextBlock { Text = "Contact" };

            // Create the tab
            var tab = new CloseableTabItem(clientManager);
            tab.SetHeader(header, "Contact");
            tab.Content = contact;

            // Add to TabControl
            tc.Items.Add(tab);
        }

        private void admin_settings(object sender, RoutedEventArgs e)
        {
            tab_windows.p_admin adm_settings = new tab_windows.p_admin();

            var header = new TextBlock { Text = "Admin Settings" };

            // Create the tab
            var tab = new CloseableTabItem(clientManager);
            tab.SetHeader(header, "Admin Settings");
            tab.Content = adm_settings;

            // Add to TabControl
            tc.Items.Add(tab);
        }

        private void infor_buttom_Click(object sender, RoutedEventArgs e)
        {
            tab_windows.p_infor inform = new tab_windows.p_infor();
            var header = new TextBlock { Text = "Information" };

            // Create the tab
            var tab = new CloseableTabItem(clientManager);
            tab.SetHeader(header, "Information");
            tab.Content = inform;

            // Add to TabControl
            tc.Items.Add(tab);
        }

        private void logout_buttom_Click(object sender, RoutedEventArgs e)
        {
            Close();
            p_log log = new p_log(clientManager);
            log.Show();
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation myDoubleAnimation1 = new DoubleAnimation();
            myDoubleAnimation1.From = 0.0;
            myDoubleAnimation1.To = 1.0;
            myDoubleAnimation1.Duration = new Duration(TimeSpan.FromSeconds(2));
            BeginAnimation(OpacityProperty, myDoubleAnimation1);

            Title = "Gold Chat: " + strName;

            dispatcherTimer.Start();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to leave the chat room?", "Gold Chat: " + strName, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
                return;
            }
            try
            {
                //Send a message to logout of the server
                Data msgToSend = new Data();
                msgToSend.cmdCommand = Command.Logout;
                msgToSend.strName = strName;
                msgToSend.strMessage = null;

                byte[] logoutMessage = msgToSend.ToByte();

                //clientManager.ClientLogin -= OnClientLogin; //unsubscribe

                var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
                anim.Completed += (s, _) => Close();
                BeginAnimation(OpacityProperty, anim);

                clientManager.LogoutSend(logoutMessage);

                clientManager.config.SaveConfig(clientManager.config);// when user exit from program, we save configuration

                clientManager = null;
            }
            catch (ObjectDisposedException ex)
            {
                MessageBox.Show(ex.Message, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddFriend(object sender, RoutedEventArgs e)
        {
            string friendName = lb_users.SelectedItem.ToString();
            if (clientList.Contains(friendName) && friendName != App.clientName)
                clientManager.SendToServer(Command.manageFriend, "Add", friendName); // There is send information to server that i add someone to friend list
        }

        private void PrivateMessage(string usernName)
        {
            pm = new private_message(usernName);
            pm.Show();
        }

        private void PrivateMsgToUser(object sender, RoutedEventArgs e)
        {
            string usernName = lb_users.SelectedItem.ToString();
            if (clientList.Contains(usernName) && usernName != App.clientName)
                PrivateMessage(usernName);
        }

        private void IgnoreUser(object sender, RoutedEventArgs e)
        {
            string usernName = lb_users.SelectedItem.ToString();
            if (clientList.Contains(usernName) && usernName != App.clientName)
                clientManager.SendToServer(Command.ignoreUser, "AddIgnore", usernName);
        }

        private void PrivateMsgToFriend(object sender, MouseButtonEventArgs e)
        {
            string friendName = lb_friend_users.SelectedItem.ToString();

            if (clientFriendsList.Contains(friendName) && clientList.Contains(friendName)) // Now if friend is in our friend list + his online(exists in clientList) 
                PrivateMessage(friendName);
            else MessageBox.Show("Your Friend " + friendName + " is offline", "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Delete_Friend(object sender, RoutedEventArgs e)
        {
            string friendName = lb_friend_users.SelectedItem.ToString();
            if (clientFriendsList.Contains(friendName) && friendName != App.clientName)
                clientManager.SendToServer(Command.manageFriend, "Delete", friendName);
        }

        private void DeleteFromIgnoreList(object sender, RoutedEventArgs e)
        {
            string strMessage = lb_ignored.SelectedItem.ToString();
            if (clientIgnoredList.Contains(strMessage))
                clientManager.SendToServer(Command.ignoreUser, "DeleteIgnore", strMessage);
        }

        private void JoinToLobbie(object sender, RoutedEventArgs e)
        {
            string strMessage = lbLobbies.SelectedItem.ToString();
            if (clientChannelsList.Contains(strMessage))
                clientManager.SendToServer(Command.joinChannel, strMessage);
        }
        //exit from joined channel, if wanna enter again => required join
        //todo check if user have opened (tab_windows.channelWindow channelPanel)
        private void ExitFromLobbie(object sender, RoutedEventArgs e)
        {
            string strMessage = lbLobbies.SelectedItem.ToString();
            if (clientChannelsJoinedList.Contains(strMessage))
                clientManager.SendToServer(Command.editChannel, strMessage);
        }
        private void DeleteChannel(object sender, RoutedEventArgs e)
        {
            string strMessage = lbJoinedChann.SelectedItem.ToString();
            if (clientChannelsList.Contains(strMessage))
            {
                //todo All think in client side (also show window to ask admin password to delete)
                //Data msgToSend = new Data();

                //msgToSend.strName = clientManager.userName;
                //msgToSend.strMessage = strMessage;
                //msgToSend.cmdCommand = Command.deleteChannel;

                //byte[] byteData = msgToSend.ToByte();
                //clientManager.BeginSend(byteData);
            }
        }

        private void EnterToJoinedChannel(object sender, MouseButtonEventArgs e)
        {
            string strMessage = lbJoinedChann.SelectedItem.ToString();
            clientManager.SendToServer(Command.enterChannel, strMessage);
        }

        private void LeaveJoinedChannel(object sender, RoutedEventArgs e)
        {
            string strMessage = lbJoinedChann.SelectedItem.ToString();
            clientManager.SendToServer(Command.leaveChannel, strMessage);
        }
    }
}