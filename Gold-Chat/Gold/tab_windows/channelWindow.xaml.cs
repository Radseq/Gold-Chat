using CommandClient;
using System;
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
        string channelName;

        public channelWindow(ClientManager cm, string tabChannelName)
        {
            InitializeComponent();
            channelName = tabChannelName;
            clientManager = cm;
            clientManager.ClientChannelMessage += OnClientChannelMessage;
        }

        private void OnClientChannelMessage(object sender, ClientEventArgs e)
        {
            throw new NotImplementedException();
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
                //Fill the info for the message to be send
                Data msgToSend = new Data();

                msgToSend.strName = clientManager.userName;
                msgToSend.strMessage = sendChannelMsg.Text;
                msgToSend.strMessage2 = channelName;
                msgToSend.cmdCommand = Command.Message;

                byte[] byteData = msgToSend.ToByte();

                clientManager.BeginSend(byteData);

                sendChannelMsg.Text = null;
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to send message to the server.", "Gold Chat: " + clientManager.userName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
