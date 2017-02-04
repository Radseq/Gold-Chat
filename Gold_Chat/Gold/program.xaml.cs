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

        //public static Socket clientSocket = App.clientSocket;   //The main client socket
        public static string strName = App.clientName;          //Name by which the user logs into the room

        private byte[] byteData = new byte[1024];

        ArrayList clientList = new ArrayList();

        public static private_message pm; //private message window

        private System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        private static ClientManager clientManager;

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
        }

        private void OnClientLogin(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                clientList.Add(e.clientLoginName);
                lb_users.Items.Refresh();
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
                    // pm.Title += e.clientFriendName;
                    pm.showPrivMessageTb.Text += e.clientFriendName + ": " + e.clientPrivMessage;
                    // pm.strMessage = e.clientFriendName;
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

                //txtChatBox.Text += "<<<" + strName + " has joined the room>>>\r\n";
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
            tab_windows.p_create_room create_room = new tab_windows.p_create_room();

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation myDoubleAnimation1 = new DoubleAnimation();
            myDoubleAnimation1.From = 0.0;
            myDoubleAnimation1.To = 1.0;
            myDoubleAnimation1.Duration = new Duration(TimeSpan.FromSeconds(2));
            BeginAnimation(OpacityProperty, myDoubleAnimation1);

            Title = "Gold Chat: " + strName;

            //The user has logged into the system so we now request the server to send
            //the names of all users who are in the chat room
            Data msgToSend = new Data();
            msgToSend.cmdCommand = Command.List;
            msgToSend.strName = strName;
            msgToSend.strMessage = null;

            byteData = msgToSend.ToByte();

            clientManager.BeginSend(byteData);
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

                clientManager.LogoutSend(logoutMessage);


                clientManager.config.SaveConfig(clientManager.config);// when user exit from program, we save configuration

                clientManager = null;

                var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
                anim.Completed += (s, _) => Close();
                BeginAnimation(OpacityProperty, anim);
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

        private void AddFriendItem(object sender, RoutedEventArgs e)
        {
            string strMessage = lb_users.SelectedItem.ToString();
            if (clientList.Contains(strMessage) && lb_users.SelectedItem.ToString() != App.clientName)
            {
                //there is send information to server that i add someone to friend list
            }
        }

        private void MessageItem(object sender, RoutedEventArgs e)
        {
            string friendName = lb_users.SelectedItem.ToString();
            if (clientList.Contains(friendName) && lb_users.SelectedItem.ToString() != App.clientName)
            {
                pm = new private_message(friendName);
                //pm.strMessage = friendName;
                pm.Show();
                //pm.Title += strMessage;
            }
        }

        private void Delete_Friend(object sender, RoutedEventArgs e)
        {
            string strMessage = lb_users.SelectedItem.ToString();
            if (clientList.Contains(strMessage))
            {
                //delete friend
            }
        }
    }
}