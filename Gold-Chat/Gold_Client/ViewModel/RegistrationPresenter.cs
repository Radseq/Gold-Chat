using CommandClient;
using Gold_Client.Model;
using Gold_Client.ViewModel.Others;
using System;
using System.Security;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Gold_Client.ViewModel
{
    public class RegistrationPresenter : ObservableObject
    {
        private string registrationEmailTB;
        private string registrationLoginTB;

        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;
        ProcessReceivedByte getMessageFromServer = ProcessReceivedByte.Instance;

        public LoginPresenter loginPresenter { private get; set; } //used to fill boxses after registration

        public SecureString SecurePassword { private get; set; }
        public SecureString SecurePasswordRepeart { private get; set; }

        public Action CloseAction;

        public RegistrationPresenter()
        {
            getMessageFromServer.ClientRegistration += OnClientRegistration;
        }

        public string RegistrationLoginTB
        {
            get { return registrationLoginTB; }
            set
            {
                registrationLoginTB = value;
                RaisePropertyChangedEvent(nameof(RegistrationLoginTB));
            }
        }

        public string RegistrationEmailTB
        {
            get { return registrationEmailTB; }
            set
            {
                registrationEmailTB = value;
                RaisePropertyChangedEvent(nameof(RegistrationEmailTB));
            }
        }

        public ICommand RegistrationCommand => new DelegateCommand(() =>
        {
            ProccesAndExecuteInputs(RegistrationLoginTB, RegistrationEmailTB);
        });

        public ICommand CleanRegistrationWindowCommand => new DelegateCommand(() =>
        {
            RegistrationEmailTB = "";
            RegistrationLoginTB = "";
        });

        private void ProccesAndExecuteInputs(string login, string email)
        {
            if (login != string.Empty && new System.Net.NetworkCredential(string.Empty, SecurePassword).Password != string.Empty && new System.Net.NetworkCredential(string.Empty, SecurePasswordRepeart).Password != string.Empty && email != string.Empty)
            {
                Regex rgx = new Regex(@"^(?=[a-z])[-\w.]{0,23}([a-zA-Z\d])$");
                bool loginRegex = rgx.IsMatch(login);

                if (login.Length < 5 && login.Length > 23)
                {
                    MessageBox.Show("Your username must be between 5 and 23 chars", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                else if (!loginRegex)
                {
                    MessageBox.Show("Your username must contain only a-z or 0-9 extample michael123", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                else if (SecurePassword.Length < 6 && SecurePasswordRepeart.Length < 6)
                {
                    MessageBox.Show("Your password must be longer than 6 chars", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (new System.Net.NetworkCredential(string.Empty, SecurePassword).Password != new System.Net.NetworkCredential(string.Empty, SecurePasswordRepeart).Password)
                {
                    MessageBox.Show("Password and Repeat Password are not the same", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (email.Length == 0)
                {
                    MessageBox.Show("Enter an email.", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);

                }
                else if (!Regex.IsMatch(email, @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"))
                {
                    MessageBox.Show("Enter a valid email.", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    App.Client.strName = login;

                    clientSendToServer.SendToServer(Command.Registration, clientSendToServer.CalculateChecksum
                        (new System.Net.NetworkCredential(string.Empty, SecurePassword).Password), email);
                    if (!ReceivePackageFromServer.IsClientStartReceive)
                        ReceivePackageFromServer.BeginReceive();
                }
            }
            else
                MessageBox.Show("Fill in the fields", "Error validation", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnClientRegistration(object sender, ClientEventArgs e)
        {
            if (e.clientRegMessage == "You has been registered")
            {
                MessageBox.Show(e.clientRegMessage, "Registration Information", MessageBoxButton.OK, MessageBoxImage.Information);
                loginPresenter.LoginTB = RegistrationLoginTB;
                loginPresenter.SecurePassword = SecurePassword;
                loginPresenter.LoginCommand.Execute(null); // When user proper register -> make autologin to show window of activation code
                CloseAction(); // And close registration window
            }
            else
            {
                MessageBox.Show(e.clientRegMessage, "Registration Information", MessageBoxButton.OK, MessageBoxImage.Information);
                App.Client.strName = null;
            };
        }
    }
}
