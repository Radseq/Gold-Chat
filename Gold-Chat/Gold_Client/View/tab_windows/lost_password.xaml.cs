using CommandClient;
using System.Windows;

namespace Gold.tab_windows
{
    /// <summary>
    /// Interaction logic for remember_password.xaml
    /// </summary>
    public partial class lost_password : Window
    {
        ClientManager clientManager;

        public lost_password(ClientManager cm)
        {
            InitializeComponent();
            clientManager = cm;

            clientManager.ClientLostPass += OnClientLostPass;
            DataObject.AddPastingHandler(codeFromEmail, OnPaste);
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
            if (!isText) return;

            var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
            enableControls(text);
        }

        private void enableControls(string text)
        {
            if (text.Length == 25)
            {
                newPass.IsEnabled = true;
                newPass2.IsEnabled = true;
                sendNewPass.IsEnabled = true;
            }
        }

        private void OnClientLostPass(object sender, ClientEventArgs e)
        {
            MessageBox.Show(e.clientChangePassMessage, "Gold Chat: " + App.clientName, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void generateCode_Click(object sender, RoutedEventArgs e)
        {
            if (emailRemember.Text != "")
                clientManager.SendToServer(Command.lostPassword, "email", emailRemember.Text);
            else
                MessageBox.Show("Write only email", "Gold Chat: " + App.clientName, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void codeFromEmail_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            enableControls(codeFromEmail.Text);
        }

        private void sendNewPass_Click(object sender, RoutedEventArgs e)
        {
            if (codeFromEmail.Text != "")
            {
                if (newPass.Password == newPass2.Password)
                    clientManager.SendToServer(Command.lostPassword, "codeFromEmail", codeFromEmail.Text, clientManager.CalculateChecksum(newPass.Password));
                else MessageBox.Show("Passwords are not same", "Gold Chat: " + App.clientName, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
                MessageBox.Show("Write only Code from email", "Gold Chat: " + App.clientName, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
