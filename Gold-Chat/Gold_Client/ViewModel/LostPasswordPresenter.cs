using CommandClient;
using System.Security;
using System.Windows;
using System.Windows.Input;

namespace Gold_Client.ViewModel
{
    public class LostPasswordPresenter : ObservableObject
    {
        private string emailTb;
        private string userCodeFromEmailTB;
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;

        public SecureString SecurePassword { private get; set; }
        public SecureString SecurePasswordRepeart { private get; set; }

        public bool IsNewPasswordEnabled { get { return UserCodeFromEmailTB.Length == 25 ? false : true; } }
        public bool IsNewPassword2Enabled { get { return UserCodeFromEmailTB.Length == 25 ? false : true; } }
        public bool IsSendPasswordButtonEnabled { get { return UserCodeFromEmailTB.Length == 25 ? false : true; } }

        public string EmailTB
        {
            get { return emailTb; }
            set
            {
                emailTb = value;
                RaisePropertyChangedEvent(nameof(EmailTB));
            }
        }

        public string UserCodeFromEmailTB
        {
            get { return userCodeFromEmailTB; }
            set
            {
                userCodeFromEmailTB = value;
                RaisePropertyChangedEvent(nameof(UserCodeFromEmailTB));
            }
        }

        public ICommand GenerateCodeCommand => new DelegateCommand(() =>
        {
            clientSendToServer.SendToServer(Command.lostPassword, "email", emailTb);
        });

        public ICommand SendNewPassCommand => new DelegateCommand(() =>
        {
            if (userCodeFromEmailTB != "")
            {
                if (SecurePassword == SecurePasswordRepeart)
                {
                    clientSendToServer.SendToServer(Command.lostPassword, "codeFromEmail", userCodeFromEmailTB, clientSendToServer.CalculateChecksum(new System.Net.NetworkCredential(string.Empty, SecurePassword).Password));
                }
                else MessageBox.Show("Passwords are not same", "Gold Chat", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
                MessageBox.Show("Write only Code from email", "Gold Chat", MessageBoxButton.OK, MessageBoxImage.Information);
        });
    }
}
