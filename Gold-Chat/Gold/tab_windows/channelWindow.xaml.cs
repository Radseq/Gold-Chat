using CommandClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gold.tab_windows
{
    /// <summary>
    /// Interaction logic for channelWindow.xaml
    /// </summary>
    public partial class channelWindow : UserControl
    {
        ClientManager clientManager;
        //string channelName;

        public string channelName { get; set; }

        List<string> channelUsersList = new List<string>(); //get form server

        public channelWindow(ClientManager cm, string tabChannelName)
        {
            InitializeComponent();
            channelName = tabChannelName;
            clientManager = cm;
            clientManager.ClientChannelMessage += OnClientChannelMessage;
            clientManager.ClientLogout += ClientLogout;
            clientManager.ClientChannelEnter += OnClientChannelEnter;
            clientManager.ClientListChannelUsers += OnClientListChannelUsers;

            clientManager.ClientChannelLeave += OnClientChannelLeave;
        }

        private void OnClientChannelLeave(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                foreach (string user in channelUsersList)
                {
                    if (user == e.clientName)
                    {
                        showMessage("<<< " + e.clientName + " has leave this channel>>>" + "\r\n");
                        channelUsersList.Remove(user);
                        channelUsers.Items.Refresh();
                        break;
                    }
                }
            }));
        }

        private void OnClientListChannelUsers(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                channelUsersList.AddRange(e.clientListMessage.Split('*'));
                channelUsersList.RemoveAt(channelUsersList.Count - 1);
                channelUsers.ItemsSource = channelUsersList;
            }));
        }

        //todo check how will work with program.cs(OnClientChannelEnter)
        private void OnClientChannelEnter(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                channelMessages.Text += "<<< " + e.clientName + " log into channel" + "\r\n";
                channelUsersList.Add(e.clientName);
                channelUsers.Items.Refresh();
            }));
        }

        private void ClientLogout(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                foreach (string user in channelUsersList)
                {
                    if (user == e.clientLogoutMessage)
                    {
                        showMessage("<<< " + e.clientLogoutMessage + " has logout>>>" + "\r\n");
                        channelUsersList.Remove(user);
                        //channelUsers.Items.Remove(e.clientLogoutMessage);
                        channelUsers.Items.Refresh();
                        break;
                    }
                }
            }));
        }

        private void OnClientChannelMessage(object sender, ClientEventArgs e)
        {
            showMessage(e.clientChannelMessage);
        }

        private void showMessage(string message)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                channelMessages.Text += message;
            }));
        }

        void EnterClicked(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (!string.IsNullOrEmpty(sendChannelMsg.Text))
                {
                    SendMessage();
                    sendChannelMsg.Clear();
                }
                else
                    MessageBox.Show("Please insert a text", "Error validation", MessageBoxButton.OK, MessageBoxImage.Information);
                e.Handled = true;
            }
        }

        private void SendMessage()
        {
            try
            {
                clientManager.SendToServer(Command.Message, sendChannelMsg.Text, channelName);

                sendChannelMsg.Text = null;
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to send message to the server.", "Gold Chat: " + App.clientName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
