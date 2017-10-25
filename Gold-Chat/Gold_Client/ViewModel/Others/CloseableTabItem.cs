using CommandClient;
using Gold_Client.Model;
using Gold_Client.View.Others;
using Gold_Client.ViewModel.Others;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Gold_Client.ViewModel
{
    public class CloseableTabItem : TabItem
    {
        /// <summary>
        /// Maybe bad written but i dont have any clue to do it better :(
        /// </summary>
        string tabControlName;
        ProcessReceivedByte getMessageFromServer = ProcessReceivedByte.Instance;
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;
        ObservableCollection<object> tb;

        bool isChannel = false;

        private void OnClientKickFromChannel(object sender, ClientEventArgs e)
        {
            if (tabControlName == e.clientChannelName && e.clientName == App.Client.strName)
            {
                MessageBox.Show($"You are {e.clientKickReason}", $"Gold Chat: {App.Client.strName}", MessageBoxButton.OK, MessageBoxImage.Information);
                removeTab();
            }
        }

        private void OnClientDeleteChannel(object sender, ClientEventArgs e)
        {
            if (e.clientChannelMsg == tabControlName && e.clientChannelMsg2 != "Deny")
            {
                removeTab();
            }
        }

        public void SetHeader(UIElement header, string name, ref ObservableCollection<object> tabControlItems, bool channel = false)
        {
            tabControlName = name;
            isChannel = channel;
            // Container for header controls
            var dockPanel = new DockPanel();
            dockPanel.Children.Add(header);

            tb = tabControlItems;

            if (isChannel)
            {
                getMessageFromServer.ClientKickFromChannel += OnClientKickFromChannel;
                getMessageFromServer.ClientDeleteChannel += OnClientDeleteChannel;
                getMessageFromServer.ClientBanFromChannel += OnClientBanFromChannel;
                getMessageFromServer.ClientChannelLeave += OnClientChannelLeave;
            }


            // Close button to remove the tab
            var closeButton = new TabCloseButton();
            closeButton.Click +=
                (sender, e) =>
                {
                    if (tabControlName != "Main")
                    {
                        tb.Remove(this);
                        if (isChannel)
                            clientSendToServer.SendToServer(Command.leaveChannel, tabControlName);
                    }
                    else
                        MessageBox.Show("Cannot close Main Tab!", $"Gold Chat: {App.Client.strName}", MessageBoxButton.OK, MessageBoxImage.Error);
                };
            dockPanel.Children.Add(closeButton);

            // Set the header
            Header = dockPanel;
        }

        private void OnClientChannelLeave(object sender, ClientEventArgs e)
        {
            if (tabControlName == e.clientChannelMsg && App.Client.strName == e.clientName)
                removeTab();
        }

        private void OnClientBanFromChannel(object sender, ClientEventArgs e)
        {
            if (e.clientName == App.Client.strName && e.clientChannelName == tabControlName)
            {
                MessageBox.Show($"You are {e.clientBanReason}", $"Gold Chat: {App.Client.strName}", MessageBoxButton.OK, MessageBoxImage.Information);
                removeTab();
            }
        }

        private void removeTab()
        {
            tb.Remove(this);
        }
    }
}
