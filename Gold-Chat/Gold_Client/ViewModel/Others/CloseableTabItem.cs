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
                MessageBox.Show("You are" + e.clientKickReason, "Gold Chat: " + App.Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);
                removeTab();
            }
        }

        private void OnClientDeleteChannel(object sender, ClientEventArgs e)
        {
            if (e.clientChannelMsg == tabControlName)
            {
                MessageBox.Show("Channel owner delete channel " + tabControlName, "Gold Chat: " + App.Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);
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
            }


            // Close button to remove the tab
            var closeButton = new TabCloseButton();
            closeButton.Click +=
                (sender, e) =>
                {
                    // var a = ((TabControl)((TabItem)sender).Parent);
                    //var tabControl = Parent as ItemsControl;
                    //tabControl.Items.Remove(this);
                    tb.Remove(this);
                    if (isChannel)
                        clientSendToServer.SendToServer(Command.leaveChannel, tabControlName);
                };
            dockPanel.Children.Add(closeButton);

            // Set the header
            Header = dockPanel;
        }

        private void OnClientBanFromChannel(object sender, ClientEventArgs e)
        {
            if (e.clientName == App.Client.strName && e.clientChannelName == tabControlName)
            {
                MessageBox.Show("You are" + e.clientBanReason, "Gold Chat: " + App.Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);
                removeTab();
            }
        }

        private void removeTab()
        {
            //var tabControl = Parent as ItemsControl;
            tb.Remove(this);
        }
    }
}
