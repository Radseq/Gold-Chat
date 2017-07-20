using CommandClient;
using Gold_Client.ViewModel.Others;
using System.Security;
using System.Windows;
using System.Windows.Input;

namespace Gold_Client.ViewModel.TabWindows
{
    public class CreateRoomPresenter : ObservableObject
    {
        private string roomNameTb;
        private string welcomeMessageTb;
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;

        public SecureString EnterSecurePassword { private get; set; }
        public SecureString EnterSecurePasswordRepeart { private get; set; }
        public SecureString AdminSecurePassword { private get; set; }
        public SecureString AdminSecurePasswordRepeart { private get; set; }

        public string RoomNameTb
        {
            get { return roomNameTb; }
            set
            {
                roomNameTb = value;
                RaisePropertyChangedEvent(nameof(RoomNameTb));
            }
        }

        public string WelcomeMessageTb
        {
            get { return welcomeMessageTb; }
            set
            {
                welcomeMessageTb = value;
                RaisePropertyChangedEvent(nameof(WelcomeMessageTb));
            }
        }

        public ICommand SendCreateChannelCommand => new DelegateCommand(() =>
        {
            if (roomNameTb.Length < 3 && roomNameTb.Length >= 10)
                MessageBox.Show("Channel Name must be between 4 and 10 chars", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (EnterSecurePassword.Length < 6 && EnterSecurePasswordRepeart.Length < 6 && EnterSecurePassword.Length > 20 && EnterSecurePasswordRepeart.Length > 20)
                MessageBox.Show("Channel password must be highter than 6 chars and less than 20", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (AdminSecurePassword.Length < 6 && AdminSecurePasswordRepeart.Length < 6 && AdminSecurePassword.Length > 20 && AdminSecurePasswordRepeart.Length > 20)
                MessageBox.Show("Channel Admin password must be highter than 6 chars and less than 20", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (new System.Net.NetworkCredential(string.Empty, EnterSecurePassword).Password != new System.Net.NetworkCredential(string.Empty, EnterSecurePasswordRepeart).Password)
                MessageBox.Show("Channel Password and Repeat Password are not the same", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (new System.Net.NetworkCredential(string.Empty, AdminSecurePassword).Password != new System.Net.NetworkCredential(string.Empty, AdminSecurePasswordRepeart).Password)
                MessageBox.Show("Channel admin Password and Repeat admin Password are not the same", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);

            else if (welcomeMessageTb.Length < 6 && welcomeMessageTb.Length > 20)
                MessageBox.Show("Channel welcome message must be highter than 6 chars and less than 20", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                clientSendToServer.SendToServer(Command.createChannel, roomNameTb,
                clientSendToServer.CalculateChecksum(new System.Net.NetworkCredential(string.Empty, EnterSecurePassword).Password),
                clientSendToServer.CalculateChecksum(new System.Net.NetworkCredential(string.Empty, AdminSecurePassword).Password), welcomeMessageTb);
            }
        });
    }
}
