using CommandClient;
using System.Windows;
using System.Windows.Controls;

namespace Gold
{
    public class CloseableTabItem : TabItem
    {
        /// <summary>
        /// Maybe bad written but i dont have any clue to do it better :(
        /// </summary>
        string tabControlName;
        private static ClientManager clientManager;
        bool isChannel = false;

        public CloseableTabItem(ClientManager cm)
        {
            clientManager = cm;
        }

        public void SetHeader(UIElement header, string name, bool channel = false)
        {
            tabControlName = name;
            isChannel = channel;
            // Container for header controls
            var dockPanel = new DockPanel();
            dockPanel.Children.Add(header);

            // Close button to remove the tab
            var closeButton = new TabCloseButton();
            closeButton.Click +=
                (sender, e) =>
                {
                    var tabControl = Parent as ItemsControl;
                    tabControl.Items.Remove(this);
                    if (isChannel)
                    {
                        Data msgToSend = new Data();
                        msgToSend.strName = App.clientName;
                        msgToSend.strMessage = tabControlName;
                        msgToSend.cmdCommand = Command.leaveChannel;

                        byte[] byteData = msgToSend.ToByte();

                        clientManager.BeginSend(byteData);
                    }
                };
            dockPanel.Children.Add(closeButton);

            // Set the header
            Header = dockPanel;
        }
    }
}
