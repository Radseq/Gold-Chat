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
using System.Windows.Controls.Primitives;
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

        private byte[] byteData = new byte[1024];

        ArrayList clientList = new ArrayList();
        ArrayList clientChannelsList = new ArrayList();         //lust of all channels, required join
        ArrayList clientChannelsJoinedList = new ArrayList();   //list of channels that i joined
        ArrayList clientFriendsList = new ArrayList();          //list of friends thats i add

        public static private_message pm; //private message window

        private System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        private static ClientManager clientManager;

        tab_windows.channelWindow channelPanel = null;  //user channel window

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
            clientManager.ClientChannelLeave += OnClientChannelLeave;
            clientManager.ClientListChannelJoined += OnClientListChannelJoined;
            //friends
            clientManager.ClientAddFriend += OnClientAddFriend;
            clientManager.ClientAcceptFriend += OnClientAcceptFriend;
            clientManager.ClientListFriends += OnClientListFriends;
            clientManager.ClientDeleteFriend += OnClientDeleteFriend;
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

        private void OnClientChannelLeave(object sender, ClientEventArgs e)
        {
            //if name of e = my name => close channel window tab
            //else send info in channel that x leave
        }

        private void OnClientChannelEnter(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (e.clientName == App.clientName && e.clientChannelMsg2 == "enter")
                {
                    channelPanel = new tab_windows.channelWindow(clientManager, e.clientChannelName);

                    var header = new TextBlock { Text = e.clientChannelName };
                    // Create the tab
                    var tab = new CloseableTabItem();
                    tab.SetHeader(header);
                    tab.Content = channelPanel;

                    // Add to TabControl
                    tc.Items.Add(tab);
                }
                else if (e.clientName != App.clientName && e.clientChannelMsg2 == "enter")
                {
                    //chect(name) if tab channel is opened 
                    //if yes,
                    //message to channel User .. enter to room 
                    if (channelPanel != null && channelPanel.channelName == e.clientChannelName)
                    {
                        channelPanel.channelMessages.Text += e.clientName + " Log into channel" + "\r\n";
                    }

                }
                else MessageBox.Show(e.sendExcepMessage, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Error);
            }));
        }

        //after we got msg from server that we/friend delete as/friend we need got list of friends
        private void OnClientDeleteFriend(object sender, ClientEventArgs e)
        {
            //the names of all users that he have in friend list
            Data msgToSendFriends = new Data();
            msgToSendFriends.cmdCommand = Command.List;
            msgToSendFriends.strName = strName;
            msgToSendFriends.strMessage = "Friends";

            byteData = msgToSendFriends.ToByte();

            clientManager.BeginSend(byteData);
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
            MessageBox.Show("You are now friend with: " + e.clientFriendName, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnClientAddFriend(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                MessageBoxResult result = MessageBox.Show("User: " + e.clientName + " want to be your friend Accept?", "Gold Chat", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    Data msgToSend = new Data();

                    msgToSend.strName = strName;
                    msgToSend.strMessage = "No";
                    msgToSend.strMessage2 = e.clientName;
                    msgToSend.cmdCommand = Command.manageFriend;

                    byte[] byteData = msgToSend.ToByte();

                    clientManager.BeginSend(byteData);
                }
                else
                {
                    Data msgToSend = new Data();

                    msgToSend.strName = strName;
                    msgToSend.strMessage = "Yes";
                    msgToSend.strMessage2 = e.clientName;
                    msgToSend.cmdCommand = Command.manageFriend;

                    byte[] byteData = msgToSend.ToByte();

                    clientManager.BeginSend(byteData);
                }
            }));
        }

        private void OnClientListChannel(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                clientChannelsList.AddRange(e.clientListChannelsMessage.Split('*'));
                clientChannelsList.RemoveAt(clientChannelsList.Count - 1);
                lbLobbies.ItemsSource = clientChannelsList;
            }));
        }

        private void OnClientJoinChannel(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (e.clientChannelMsg2 == "Send Password" || e.clientChannelMsg2 == "Wrong Password")
                {
                    //i need programmatically create window


                    serverAsk sa = new serverAsk(clientManager, e.clientChannelMsg, e.clientChannelMsg2);
                    sa.Show();
                }
                else
                {
                    MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Information);
                    MessageBox.Show(e.clientChannelMsg2, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }));
        }


        private void OnClientExitChannel(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Information);
            }));
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
            Dispatcher.BeginInvoke((Action)(() =>
            {
                MessageBox.Show(e.clientChannelMsg, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Information);
            }));
        }

        private void OnClientLogin(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                clientList.Add(e.clientLoginName);
                lb_users.Items.Refresh();
                txtChatBox.Text += e.clientLoginMessage + "\r\n";
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
                clientList.AddRange(e.clientListMessage.Split('*'));
                clientList.RemoveAt(clientList.Count - 1);
                lb_users.ItemsSource = clientList;
            }));
        }

        private void OnClientLogout(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                clientList.Remove(e.clientLogoutMessage);
                lb_users.Items.Refresh();
            }));
        }

        /// <summary>
        /// Close tab window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnClick(object sender, RoutedEventArgs args)
        {
            CheckBox selectionCheckBox = sender as CheckBox;
            if (selectionCheckBox != null && selectionCheckBox.IsChecked == true)
            {
                foreach (Control child in DesignerCanvas.Children)
                {
                    Selector.SetIsSelected(child, true);
                }
            }
            else
            {
                foreach (Control child in DesignerCanvas.Children)
                {
                    Selector.SetIsSelected(child, false);
                }
            }
        }

        private void SendMessage()
        {
            try
            {
                //Fill the info for the message to be send
                Data msgToSend = new Data();

                msgToSend.strName = strName;
                msgToSend.strMessage = tb_message.Text;
                msgToSend.cmdCommand = Command.Message;

                byte[] byteData = msgToSend.ToByte();

                clientManager.BeginSend(byteData);

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
                //Fill the info for the message to be send
                Data msgToSend = new Data();

                msgToSend.strName = strName;
                msgToSend.strMessage = pm.sendPrivMessageTb.Text;
                msgToSend.strMessage2 = pm.strMessage; //friend name
                msgToSend.cmdCommand = Command.privMessage;

                byte[] byteData = msgToSend.ToByte();

                //Send it to the server
                clientManager.BeginSend(byteData);
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
            var tab = new CloseableTabItem();
            tab.SetHeader(header);
            tab.Content = ctrl_panel;

            // Add to TabControl
            tc.Items.Add(tab);
        }

        private void create_room_buttom_Click(object sender, RoutedEventArgs e)
        {
            tab_windows.p_create_room create_room = new tab_windows.p_create_room(clientManager);

            var header = new TextBlock { Text = "Create Room" };

            // Create the tab
            var tab = new CloseableTabItem();
            tab.SetHeader(header);
            tab.Content = create_room;

            // Add to TabControl
            tc.Items.Add(tab);
        }

        private void suggest_buttom_Click(object sender, RoutedEventArgs e)
        {
            tab_windows.p_suggest suggest = new tab_windows.p_suggest();
            var header = new TextBlock { Text = "Suggestions" };

            // Create the tab
            var tab = new CloseableTabItem();
            tab.SetHeader(header);
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
            var tab = new CloseableTabItem();
            tab.SetHeader(header);
            tab.Content = abouse;

            // Add to TabControl
            tc.Items.Add(tab);
        }

        private void contact_buttom_Click(object sender, RoutedEventArgs e)
        {
            tab_windows.p_contact contact = new tab_windows.p_contact();

            var header = new TextBlock { Text = "Contact" };

            // Create the tab
            var tab = new CloseableTabItem();
            tab.SetHeader(header);
            tab.Content = contact;

            // Add to TabControl
            tc.Items.Add(tab);
        }

        private void admin_settings(object sender, RoutedEventArgs e)
        {
            tab_windows.p_admin adm_settings = new tab_windows.p_admin();

            var header = new TextBlock { Text = "Admin Settings" };

            // Create the tab
            var tab = new CloseableTabItem();
            tab.SetHeader(header);
            tab.Content = adm_settings;

            // Add to TabControl
            tc.Items.Add(tab);
        }

        private void infor_buttom_Click(object sender, RoutedEventArgs e)
        {
            tab_windows.p_infor inform = new tab_windows.p_infor();
            var header = new TextBlock { Text = "Information" };

            // Create the tab
            var tab = new CloseableTabItem();
            tab.SetHeader(header);
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

        //TODO client manager receive takes only list and channel
        // friends and ChannelsJoined wont
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation myDoubleAnimation1 = new DoubleAnimation();
            myDoubleAnimation1.From = 0.0;
            myDoubleAnimation1.To = 1.0;
            myDoubleAnimation1.Duration = new Duration(TimeSpan.FromSeconds(2));
            BeginAnimation(OpacityProperty, myDoubleAnimation1);

            Title = "Gold Chat: " + strName;

            //the names of all channels that he have joined
            Data msgToSendJoinedChannels = new Data();
            msgToSendJoinedChannels.cmdCommand = Command.List;
            msgToSendJoinedChannels.strName = strName;
            msgToSendJoinedChannels.strMessage = "ChannelsJoined";

            byte[] byteDataD = msgToSendJoinedChannels.ToByte();

            clientManager.BeginSend(byteDataD);

            //the names of all users that he have in friend list
            Data msgToSendFriends = new Data();
            msgToSendFriends.cmdCommand = Command.List;
            msgToSendFriends.strName = strName;
            msgToSendFriends.strMessage = "Friends";

            byte[] byteDataC = msgToSendFriends.ToByte();

            clientManager.BeginSend(byteDataC);

            //The user has logged into the system so we now request the server to send
            //the names of all users who are in the chat room
            Data msgToSend = new Data();
            msgToSend.cmdCommand = Command.List;
            msgToSend.strName = strName;
            msgToSend.strMessage = null;

            byte[] byteDataA = msgToSend.ToByte();

            clientManager.BeginSend(byteDataA);

            //the names of all channels that he have acces to
            Data msgToSendChannel = new Data();
            msgToSendChannel.cmdCommand = Command.List;
            msgToSendChannel.strName = strName;
            msgToSendChannel.strMessage = "Channel";

            byte[] byteDataB = msgToSendChannel.ToByte();

            clientManager.BeginSend(byteDataB);
        }

        #region Functions

        /// <summary>
        /// Notify the user when receive the file completely.
        /// </summary>
        public void FileReceiveDone()
        {
            // MessageBox.Show(this, Properties.Resources.FileReceivedDoneMsg);
        }

        /// <summary>
        /// Notify the user when connect to the server successfully.
        /// </summary>
        public void ConnectDone()
        {
            //MessageBox.Show(this, Properties.Resources.ConnectionMsg);
        }

        #endregion

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to leave the chat room?", "Gold Chat", MessageBoxButton.YesNo, MessageBoxImage.Question);
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
                clientManager.IsUserLogout = true;

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
            if (clientList.Contains(friendName) && lb_users.SelectedItem.ToString() != App.clientName)
            {
                //there is send information to server that i add someone to friend list
                Data msgToSend = new Data();

                msgToSend.strName = clientManager.userName; //channel admin
                msgToSend.strMessage = "Add";
                msgToSend.strMessage2 = friendName;
                msgToSend.cmdCommand = Command.manageFriend;

                byte[] byteData = msgToSend.ToByte();
                clientManager.BeginSend(byteData);
            }
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
            {
                PrivateMessage(usernName);
            }
        }

        private void PrivateMsgToFriend(object sender, MouseButtonEventArgs e)
        {
            string friendName = lb_friend_users.SelectedItem.ToString();
            //Now if friend is in our friend list + his online(exists in clientList) 
            if (clientFriendsList.Contains(friendName) /*&& friendName != App.clientName why i fucking write that when we newer be in lb_friend_users*/ && clientList.Contains(friendName))
            {
                PrivateMessage(friendName);
            }
            else MessageBox.Show("Your Friend " + friendName + " is offline", "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Delete_Friend(object sender, RoutedEventArgs e)
        {
            string friendName = lb_friend_users.SelectedItem.ToString();
            if (clientFriendsList.Contains(friendName) && lb_friend_users.SelectedItem.ToString() != App.clientName)
            {
                Data msgToSend = new Data();

                msgToSend.strName = clientManager.userName; //channel admin
                msgToSend.strMessage = "Delete";
                msgToSend.strMessage2 = friendName;
                msgToSend.cmdCommand = Command.manageFriend;

                byte[] byteData = msgToSend.ToByte();
                clientManager.BeginSend(byteData);
            }
        }

        private void JoinToLobbie(object sender, RoutedEventArgs e)
        {
            string strMessage = lbLobbies.SelectedItem.ToString();
            if (clientChannelsList.Contains(strMessage))
            {
                Data msgToSend = new Data();

                msgToSend.strName = clientManager.userName; //channel admin
                msgToSend.strMessage = strMessage;
                //msgToSend.strMessage2 = clientManager.CalculateChecksum(enterPass.Password); // haslo
                msgToSend.cmdCommand = Command.joinChannel;

                byte[] byteData = msgToSend.ToByte();
                clientManager.BeginSend(byteData);
            }
        }
        private void ExitFromLobbie(object sender, RoutedEventArgs e)
        {
            string strMessage = lbLobbies.SelectedItem.ToString();
            if (clientChannelsList.Contains(strMessage))
            {

            }
        }
        private void DeleteLobbie(object sender, RoutedEventArgs e)
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

            Data msgToSend = new Data();

            msgToSend.strName = clientManager.userName;
            msgToSend.strMessage = strMessage;
            msgToSend.cmdCommand = Command.enterChannel;

            byte[] byteData = msgToSend.ToByte();
            clientManager.BeginSend(byteData);
        }

        private void LeaveJoinedChannel(object sender, RoutedEventArgs e)
        {
            string strMessage = lbJoinedChann.SelectedItem.ToString();
            // if (clientChannelsList.Contains(strMessage))
            //{
            Data msgToSend = new Data();

            msgToSend.strName = clientManager.userName;
            msgToSend.strMessage = strMessage;
            msgToSend.cmdCommand = Command.leaveChannel;

            byte[] byteData = msgToSend.ToByte();
            clientManager.BeginSend(byteData);
            // }
        }
    }
}