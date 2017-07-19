using CommandClient;
using Gold_Client.Model;
using System;
using System.Windows;
using System.Windows.Input;

namespace Gold_Client.ViewModel.Others
{
    public class ActivationUserPresenter : ObservableObject
    {
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;
        ProcessReceivedByte proccesReceiverInformation = ProcessReceivedByte.Instance;

        public Action CloseAction;

        string activationCodeTb;

        public ActivationUserPresenter()
        {
            proccesReceiverInformation.ClientSendActivCodeFromEmail += OnClientSendActivCodeFromEmail;
        }

        private void OnClientSendActivCodeFromEmail(object sender, ClientEventArgs e)
        {
            if (e.clientSendActivCodeFromEmail == "Now you can login into application")
            {
                MessageBoxResult result = MessageBox.Show(e.clientSendActivCodeFromEmail, "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
                if (result == MessageBoxResult.OK)
                    CloseAction();
            }
        }

        public string ActivationCodeTb
        {
            get { return activationCodeTb; }
            set
            {
                activationCodeTb = value;
                RaisePropertyChangedEvent(nameof(ActivationCodeTb));
            }
        }

        public ICommand ActivationCommand => new DelegateCommand(() =>
        {
            if (!string.IsNullOrWhiteSpace(ActivationCodeTb))
            {
                clientSendToServer.SendToServer(Command.SendActivationCode, ActivationCodeTb);
            }
            else
                MessageBox.Show("Login or password cant be null", "Login Information", MessageBoxButton.OK, MessageBoxImage.Error);
        });


        public ICommand ReSendActivationToEmailCommand => new DelegateCommand(() =>
        {
            clientSendToServer.SendToServer(Command.SendActivationCode, "");
        });
    }
}
