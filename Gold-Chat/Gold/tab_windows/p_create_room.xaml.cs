using CommandClient;
using System.Windows;
using System.Windows.Controls;

namespace Gold.tab_windows
{
    /// <summary>
    /// Interaction logic for p_create_room.xaml
    /// </summary>
    public partial class p_create_room : UserControl
    {
        ClientManager clientManager;

        public p_create_room(ClientManager cm)
        {
            InitializeComponent();
            clientManager = cm;
        }

        private void create_channel_Click(object sender, RoutedEventArgs e)
        {
            if (roomNameTb.Text != string.Empty && enterPass.Password != string.Empty && confEnterPass.Password != string.Empty && amdinPass.Password != string.Empty
                && confAdminPass.Password != string.Empty && welcomeMessageTb.Text != string.Empty)
            {
                if (roomNameTb.Text.Length < 3 && roomNameTb.Text.Length >= 10)
                {

                    MessageBox.Show("Channel Name must be between 4 and 10 chars", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    roomNameTb.Focus();
                }

                else if (enterPass.Password.Length < 6 && confEnterPass.Password.Length < 6 && enterPass.Password.Length > 20 && confEnterPass.Password.Length > 20)
                {

                    MessageBox.Show("Channel password must be highter than 6 chars and less than 20", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    enterPass.Focus();
                }

                else if (amdinPass.Password.Length < 6 && confAdminPass.Password.Length < 6 && amdinPass.Password.Length > 20 && confAdminPass.Password.Length > 20)
                {

                    MessageBox.Show("Channel Admin password must be highter than 6 chars and less than 20", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    amdinPass.Focus();
                }

                else if (enterPass.Password != confEnterPass.Password)
                {

                    MessageBox.Show("Channel Password and Repeat Password are not the same", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    enterPass.Focus();
                }

                else if (amdinPass.Password != confAdminPass.Password)
                {

                    MessageBox.Show("Channel admin Password and Repeat admin Password are not the same", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    amdinPass.Focus();
                }

                else if (welcomeMessageTb.Text.Length < 6 && welcomeMessageTb.Text.Length > 20)
                {
                    MessageBox.Show("Channel welcome message must be highter than 6 chars and less than 20", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    welcomeMessageTb.Focus();

                }
                else
                    clientManager.SendToServer(Command.createChannel, roomNameTb.Text, clientManager.CalculateChecksum(enterPass.Password), clientManager.CalculateChecksum(amdinPass.Password), welcomeMessageTb.Text);

            }
            else
                MessageBox.Show("Fill in the fields", "Error validation", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
