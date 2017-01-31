using System;
using System.Windows;
using System.Windows.Input;

namespace Gold
{
    /// <summary>
    /// Interaction logic for private_message.xaml
    /// </summary>
    public partial class private_message : Window
    {
        //private Socket clientSocket = App.clientSocket;
        public string strMessage;

        private string text;

        private byte[] byteData = new byte[1024];

        public string Text
        {
            get
            {
                return text;
                //textBox.Text = text;
            }

            set
            {
                text = value;
                sendPrivMessageTb.Text = text;
            }
        }

        public private_message()
        {
            InitializeComponent();
            //FriendSocket = program.
            Title += strMessage;

        }

        void EnterClicked(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (!string.IsNullOrEmpty(sendPrivMessageTb.Text))
                {
                    program.SendPrivMessage();
                    //showPrivMessageTb.Text += sendPrivMessageTb.Text;
                    sendPrivMessageTb.Clear();
                }
                else
                    MessageBox.Show("Please insert a text", "Error validation", MessageBoxButton.OK, MessageBoxImage.Information);
                e.Handled = true;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            program.pm = null;
        }
    }
}
