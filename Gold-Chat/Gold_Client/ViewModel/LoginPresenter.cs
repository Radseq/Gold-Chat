using Gold_Client.View;
using Gold_Client.ViewModel.Others;
using System;
using System.Security;
using System.Windows;
using System.Windows.Input;
using Gold_Client.Model;

namespace Gold_Client.ViewModel
{
    public class LoginPresenter : ObservableObject
    {
        private string login;

        ClientReceivedFromServer clientReceive = ClientReceivedFromServer.Instance;
        ProcessReceivedByte proccesReceiverInformation = new ProcessReceivedByte();

        public SecureString SecurePassword { private get; set; }

        public Action CloseAction { get; set; }

        public LoginPresenter()
        {
            proccesReceiverInformation.ProccesBuffer();
            proccesReceiverInformation.ClientSuccesLogin += OnClientSuccesLogin;
        }

        private void OnClientSuccesLogin(object sender, ClientEventArgs e)
        {
            CloseAction();
        }

        public string LoginTB
        {
            get { return login; }
            set
            {
                login = value;
                RaisePropertyChangedEvent(nameof(LoginTB));
            }
        }

        public ICommand LoginCommand => new DelegateCommand(p =>
        {
            if (!string.IsNullOrWhiteSpace(login) && !string.IsNullOrWhiteSpace(new System.Net.NetworkCredential(string.Empty, SecurePassword).Password))
            {
                ClientLogin clientLogin = new ClientLogin();
                clientLogin.SendLoginAndEncryptPass(login, SecurePassword);
            }
            else
                MessageBox.Show("Login or password cant be null", "Login Information", MessageBoxButton.OK, MessageBoxImage.Error);

        });

        public ICommand RegistrationWindowCommand => new DelegateCommand(p =>
        {
            RegistrationWindow registration = new RegistrationWindow();
            registration.Show();

        });

        public ICommand LostPasswordWindowCommand => new DelegateCommand(p =>
        {
            LostPasswordWindow lostPassWindow = new LostPasswordWindow();
            lostPassWindow.Show();
        });
    }
}
