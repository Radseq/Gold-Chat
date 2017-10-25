using CommandClient;
using Gold_Client.Model;
using Gold_Client.ViewModel.Others;
using System;
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
        ProcessReceivedByte getMessageFromServer = ProcessReceivedByte.Instance;

        public SecureString SecurePassword { private get; set; }
        public SecureString SecurePasswordRepeart { private get; set; }

        public Action CloseAction;

        bool isNewPasswordEnabled = false;
        bool isNewPassword2Enabled = false;
        bool isSendPasswordButtonEnabled = false;

        public bool IsNewPasswordEnabled
        {
            get { return isNewPasswordEnabled; }
            set
            {
                isNewPasswordEnabled = value; RaisePropertyChangedEvent(nameof(IsNewPasswordEnabled));
            }
        }

        public bool IsNewPassword2Enabled
        {
            get { return isNewPassword2Enabled; }
            set { isNewPassword2Enabled = value; RaisePropertyChangedEvent(nameof(IsNewPassword2Enabled)); }
        }

        public bool IsSendPasswordButtonEnabled
        {
            get { return isSendPasswordButtonEnabled; }
            set { isSendPasswordButtonEnabled = value; RaisePropertyChangedEvent(nameof(IsSendPasswordButtonEnabled)); }
        }

        public LostPasswordPresenter()
        {
            getMessageFromServer.ClientLostPass += OnClientLostPass;
        }

        private void OnClientLostPass(object sender, ClientEventArgs e)
        {
            MessageBox.Show(e.clientMessage, $"Gold Chat: {App.Client.strName}", MessageBoxButton.OK, MessageBoxImage.Information);
            if (e.clientMessage == "Your Password has been changed!")
                CloseAction();
        }

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
                enableButtonsAndPasswordBoxIfLenght25();
                RaisePropertyChangedEvent(nameof(UserCodeFromEmailTB));
            }
        }

        private void enableButtonsAndPasswordBoxIfLenght25()
        {
            if (UserCodeFromEmailTB.Length == 25)
            {
                IsNewPasswordEnabled = true;
                IsNewPassword2Enabled = true;
                IsSendPasswordButtonEnabled = true;
            }
            else
            {
                IsNewPasswordEnabled = false;
                IsNewPassword2Enabled = false;
                IsSendPasswordButtonEnabled = false;
            }
        }

        public ICommand GenerateCodeCommand => new DelegateCommand(() =>
        {
            if (!string.IsNullOrWhiteSpace(emailTb))
            {
                clientSendToServer.SendToServer(Command.lostPassword, "email", emailTb);
                StartReceive();
            }
            else
                MessageBox.Show("Write email", "Gold Chat", MessageBoxButton.OK, MessageBoxImage.Error);
        });

        public ICommand SendNewPassCommand => new DelegateCommand(() =>
        {
            if (!string.IsNullOrWhiteSpace(userCodeFromEmailTB))
            {
                if (new System.Net.NetworkCredential(string.Empty, SecurePassword).Password == (new System.Net.NetworkCredential(string.Empty, SecurePasswordRepeart).Password))
                {
                    clientSendToServer.SendToServer(Command.lostPassword, "codeFromEmail", userCodeFromEmailTB, clientSendToServer.CalculateChecksum(new System.Net.NetworkCredential(string.Empty, SecurePassword).Password));
                    StartReceive();
                }
                else MessageBox.Show("Passwords are not same", "Gold Chat", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
                MessageBox.Show("Write only Code from email", "Gold Chat", MessageBoxButton.OK, MessageBoxImage.Information);
        });

        private void StartReceive()
        {
            if (!ReceivePackageFromServer.IsClientStartReceive)
                ReceivePackageFromServer.BeginReceive();
        }

    }
}
