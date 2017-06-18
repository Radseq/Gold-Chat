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

        bool isChannel = false;

        private void OnClientKickFromChannel(object sender, ClientEventArgs e)
        {
            if (tabControlName == e.clientChannelName)
            {
                var tabControl = Parent as ItemsControl;
                tabControl.Items.Remove(this);
            }
        }

        public void SetHeader(UIElement header, string name, ref ObservableCollection<object> tabControlItems, bool channel = false)
        {
            tabControlName = name;
            isChannel = channel;
            // Container for header controls
            var dockPanel = new DockPanel();
            dockPanel.Children.Add(header);

            ObservableCollection<object> tb = tabControlItems;

            if (isChannel)
                getMessageFromServer.ClientKickFromChannel += OnClientKickFromChannel;
            

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
    }
}
