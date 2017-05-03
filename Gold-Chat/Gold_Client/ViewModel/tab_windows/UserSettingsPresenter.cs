using CommandClient;
using System.Security;
using System.Windows.Input;

namespace Gold_Client.ViewModel.tab_windows
{
    public class UserSettingsPresenter : ObservableObject
    {
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;

        public SecureString SecurePassword { private get; set; }
        public SecureString SecurePasswordRepeart { private get; set; }

        private bool isSelected;

        public bool IsChecked
        {
            get
            {
                return isSelected;
            }
            set
            {
                if (isSelected == value)
                {
                    return;
                }
                isSelected = value;
                RaisePropertyChangedEvent(nameof(IsChecked));
            }
        }

        public UserSettingsPresenter()
        {
            Configuration config = new Configuration();
            if (config.loginEmailNotyfication == false)
                IsChecked = false;
            else IsChecked = true;
        }

        public ICommand SaveUserSettingsCommand => new DelegateCommand(() =>
        {
            if (SecurePassword != null && SecurePasswordRepeart != null && SecurePassword.Length > 6)
                clientSendToServer.SendToServer(Command.changePassword, clientSendToServer.CalculateChecksum(new System.Net.NetworkCredential(string.Empty, SecurePassword).Password));
        });
    }
}
