//using Server;
using CommandClient;
using System;
using System.Collections;
using System.ComponentModel;
//chat
//using Gold.Utils;
//voice
using System.Net.Sockets;
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

        public static Socket clientSocket = App.clientSocket;   //The main client socket
        public static string strName = App.clientName;          //Name by which the user logs into the room

        private byte[] byteData = new byte[1024];

        ArrayList clientList = new ArrayList();

        public static private_message pm; //private message window

        private System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        public program()
        {
            InitializeComponent();
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

                //Send it to the server
                clientSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnSend), null);

                tb_message.Text = null;
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to send message to the server.", "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void SendPrivMessage()
        {
            try
            {
                //Fill the info for the message to be send
                Data msgToSend = new Data();

                msgToSend.strName = strName;
                msgToSend.friendName = pm.friendName;
                msgToSend.strMessage = pm.sendPrivMessageTb.Text;
                msgToSend.cmdCommand = Command.privMessage;

                byte[] byteData = msgToSend.ToByte();

                //give own message to showed windows of private message
                pm.showPrivMessageTb.Text += strName + ": " + pm.sendPrivMessageTb.Text + "\r\n";

                //Send it to the server
                clientSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnSend), null);

                pm.sendPrivMessageTb.Text = null;
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to send message to the server.", "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndReceive(ar);
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    Data msgReceived = new Data(byteData);
                    //Accordingly process the message received
                    switch (msgReceived.cmdCommand)
                    {
                        case Command.Login:
                            clientList.Add(msgReceived.strName);
                            lb_users.Items.Refresh();
                            break;

                        case Command.Logout:
                            clientList.Remove(msgReceived.strName);
                            lb_users.Items.Refresh();
                            break;

                        case Command.Message:
                            break;

                        case Command.privMessage:
                            break;

                        case Command.List:
                            clientList.AddRange(msgReceived.strMessage.Split('*'));
                            clientList.RemoveAt(clientList.Count - 1);
                            lb_users.ItemsSource = clientList;

                            txtChatBox.Text += "<<<" + strName + " has joined the room>>>\r\n";
                            break;
                        case Command.Life:
                            break;
                    }
                    // Procedure listening for server messages.
                    if (msgReceived.strMessage != null && msgReceived.cmdCommand != Command.List && msgReceived.cmdCommand != Command.privMessage)
                        txtChatBox.Text += msgReceived.strMessage + "\r\n";
                    else if (msgReceived.strMessage != null && msgReceived.cmdCommand == Command.privMessage && msgReceived.cmdCommand != Command.List && msgReceived.friendName != null)
                    {
                        if (pm != null)
                        {
                            pm.showPrivMessageTb.Text += msgReceived.strMessage + "\r\n";
                        }
                        else
                        {
                            pm = new private_message();
                            pm.Show();
                            pm.Title += msgReceived.strName;
                            pm.showPrivMessageTb.Text += msgReceived.strMessage + "\r\n";
                            pm.friendName = msgReceived.strName;
                        }

                    }

                    byteData = new byte[1024];

                    clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
                }));
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Error);
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
            tab_windows.p_user ctrl_panel = new tab_windows.p_user();

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
            p_log log = new p_log();
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

            clientSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnSend), null);

            byteData = new byte[1024];
            //Start listening to the data asynchronously
            clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
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
                clientSocket.Send(logoutMessage, 0, logoutMessage.Length, SocketFlags.None);
                clientSocket.Close();
                var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
                anim.Completed += (s, _) => Close();
                BeginAnimation(OpacityProperty, anim);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Gold Chat: " + strName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lb_users_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string friendName = lb_users.SelectedItem.ToString();
            if (clientList.Contains(friendName) && lb_users.SelectedItem.ToString() != App.clientName)
            {
                pm = new private_message();
                pm.Show();
                pm.friendName = friendName;
                pm.Title += friendName;
            }
        }
    }
}