using CommandClient;
using System.Security;
using System.Windows;
using System.Windows.Input;

namespace Gold_Client.ViewModel
{
    public class LostPasswordPresenter : ObservableObject
    {
        private string emailTb;
        private string userCodeFromEmailTB = "0";
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;

        public SecureString SecurePassword { private get; set; }
        public SecureString SecurePasswordRepeart { private get; set; }

        public bool IsNewPasswordEnabled { get { return userCodeFromEmailTB.Length == 25 ? true : false; } }
        public bool IsNewPassword2Enabled { get { return userCodeFromEmailTB.Length == 25 ? true : false; } }
        public bool IsSendPasswordButtonEnabled { get { return userCodeFromEmailTB.Length == 25 ? true : false; } }

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
            if (!string.IsNullOrWhiteSpace(emailTb))
                clientSendToServer.SendToServer(Command.lostPassword, "email", emailTb);
            else
                MessageBox.Show("Write email", "Gold Chat", MessageBoxButton.OK, MessageBoxImage.Error);
        });

        public ICommand SendNewPassCommand => new DelegateCommand(() =>
        {
            if (!string.IsNullOrWhiteSpace(userCodeFromEmailTB))
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
