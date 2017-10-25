using CommandClient;
using Gold_Client.Model;
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

        Configuration<Settings> config = new Configuration<Settings>();
        Settings userSettings = new Settings();

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
                    return;
                loginNotyfiIsChecked = value;
                RaisePropertyChangedEvent(nameof(LoginNotyfiIsChecked));
            }
        }

        public UserSettingsPresenter()
        {
            proccesReceiverInformation.ClientChangePass += (s, e) => MessageBox.Show(e.clientChangePassMessage, $"Gold Chat: {App.Client.strName}", MessageBoxButton.OK, MessageBoxImage.Information);
            userSettings = config.LoadConfig(userSettings);
            if (userSettings.loginEmailNotyfication == false)
                LoginNotyfiIsChecked = false;
            else LoginNotyfiIsChecked = true;
        }

        public ICommand SaveUserSettingsCommand => new DelegateCommand(() =>
        {
            if (SecurePassword != null && SecurePasswordRepeart != null && SecurePassword.Length > 6)
                clientSendToServer.SendToServer(Command.changePassword, clientSendToServer.CalculateChecksum(new System.Net.NetworkCredential(string.Empty, SecurePassword).Password));
            userSettings.loginEmailNotyfication = LoginNotyfiIsChecked;
            config.SaveConfig(userSettings);
        });
    }
}
