using CommandClient;
using Gold_Client.ViewModel.Others;
using System.Security;
using System.Windows;
using System.Windows.Input;

namespace Gold_Client.ViewModel.TabWindows
{
    public class UserSettingsPresenter : ObservableObject
    {
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;
        ProcessReceivedByte proccesReceiverInformation = ProcessReceivedByte.Instance;

        public SecureString SecurePassword { private get; set; }
        public SecureString SecurePasswordRepeart { private get; set; }

        private bool loginNotyfiIsChecked;

        public bool LoginNotyfiIsChecked
        {
            get
            {
                return loginNotyfiIsChecked;
            }
            set
            {
                if (loginNotyfiIsChecked == value)
                {
                    return;
                }
                loginNotyfiIsChecked = value;
                RaisePropertyChangedEvent(nameof(LoginNotyfiIsChecked));
            }
        }

        public UserSettingsPresenter()
        {
            proccesReceiverInformation.ClientChangePass += (s, e) => MessageBox.Show(e.clientChangePassMessage, "Gold Chat: " + User.strName, MessageBoxButton.OK, MessageBoxImage.Information);

            Configuration config = new Configuration();
            if (config.loginEmailNotyfication == false)
                LoginNotyfiIsChecked = false;
            else LoginNotyfiIsChecked = true;
        }

        public ICommand SaveUserSettingsCommand => new DelegateCommand(() =>
        {
            if (SecurePassword != null && SecurePasswordRepeart != null && SecurePassword.Length > 6)
                clientSendToServer.SendToServer(Command.changePassword, clientSendToServer.CalculateChecksum(new System.Net.NetworkCredential(string.Empty, SecurePassword).Password));
        });
    }
}
