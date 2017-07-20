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
        public string strMessage;//name of friend

        public private_message(string friendName)
        {
            InitializeComponent();
            strMessage = friendName;
            Title += friendName;
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
